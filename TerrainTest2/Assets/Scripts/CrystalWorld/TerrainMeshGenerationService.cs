using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CrystalWorld {

	public class TerrainMeshGenerationService : BaseMeshInfoService, ICellMeshInfoGenerator {

		private struct PositionTerrainInfo {
			public Vector3 position;
			public int terrainGroup;
			public int terrainValue;

			public PositionTerrainInfo (Vector3 pos, int tGroup, int tValue) {
				position = pos;
				terrainGroup = tGroup;
				terrainValue = tValue;
			}
		}

		private struct Edge {
			public int i1, i2;

			public Edge (int index1, int index2) {
				i1 = index1;
				i2 = index2;
			}

			public override string ToString() {
				return i1.ToString () + "/" + i2.ToString ();
			}
		}

		private static readonly Edge[] Edges4 = new Edge[] {
			new Edge(0,1), new Edge(0,2), new Edge(0,3), // base to border
			new Edge(1,2), new Edge(2,3), new Edge(3,1) // border
		};

		private static readonly Edge[] Edges6 = new Edge[] {
			new Edge(0,1), new Edge(0,2), new Edge(0,3), new Edge(0,4), // base to rim
			new Edge(5,1), new Edge(5,2), new Edge(5,3), new Edge(5,4), // far to rim
			new Edge(1,2), new Edge(2,3), new Edge(3,4), new Edge(4,1)  // rim
		};

		private float _positionTolerance;
		private int _terrainGroup;

		public int TerrainGroup {
			get { return _terrainGroup; }
			set { _terrainGroup = value; }
		}

		public TerrainMeshGenerationService ( IBlockService blockService, ITerrainService terrainService, IDistortionService distortionService)
			: base (blockService, terrainService, distortionService) {
		}

		public MeshInfo GenerateMeshInfo (CellInfo cell, Vector3 vertexOffset) {

			_positionTolerance = this.BlockService.Spacing * 0.1f;

			List<Index3> indices3 = new List<Index3> ();
			List<PositionTerrainInfo> allData = new List<PositionTerrainInfo> ();

			indices3.Add(cell.step);
			indices3.Add(this.BlockService.GetNeighborStep (indices3[0], Neighbors.TOP_NORTH_EAST));
			indices3.Add(this.BlockService.GetNeighborStep (indices3[0], Neighbors.NORTH_EAST));
			indices3.Add(this.BlockService.GetNeighborStep (indices3[0], Neighbors.NORTH_WEST));
			indices3.Add(this.BlockService.GetNeighborStep (indices3[0], Neighbors.TOP_NORTH_WEST));
			indices3.Add(this.BlockService.GetNeighborStep (indices3[1], Neighbors.NORTH_WEST));
			indices3.Add(this.BlockService.GetNeighborStep (indices3[0], Neighbors.EAST));
			indices3.Add(this.BlockService.GetNeighborStep (indices3[0], Neighbors.TOP_SOUTH));

			var basePos = this.BlockService.GetPosition (indices3 [0]);
			foreach (var i in indices3) {
				var pos = this.BlockService.GetPosition (i);
				var grp = this.TerrainService.GetTerrainGroup (pos);
				var val = this.TerrainService.GetTerrainValue (pos, grp);
				if (this.DistortionService != null) {
					pos = pos + this.DistortionService.GetDistortionAtPosition (pos);
				}
				allData.Add (new PositionTerrainInfo(pos - basePos, grp, val));
			}

			// 6-point base segment
			var currData = new PositionTerrainInfo[] { allData [0], allData [1], allData [2], allData [3], allData [4], allData [5] };
			var mi = BuildSegmentMesh (currData);
			if (!(mi.IsValid)) {
				mi.Init ();
			}
			// first 4-point segment
			currData = new PositionTerrainInfo[] { allData [0], allData [1], allData [2], allData [6] };
			mi.Append(BuildSegmentMesh (currData), _positionTolerance);
			// second 4-point segment
			currData = new PositionTerrainInfo[] { allData [0], allData [1], allData [4], allData [7] };
			mi.Append(BuildSegmentMesh (currData), _positionTolerance);

			if (mi.IsValid) {
				mi.vertices = mi.vertices.Select (x => x + vertexOffset).ToList();
			}

			return mi;
		}

		private MeshInfo BuildSegmentMesh (PositionTerrainInfo[] data) {
			var minority = FindMinority (data);

			if (minority.Count > 0) {
				switch (data.Length) {
				case 4:
					return Build4SegmentMesh (data, minority);
				case 6:
					return Build6SegmentMesh (data, minority);
				}
			}
			return new MeshInfo ();

		}

		private MeshInfo Build4SegmentMesh(PositionTerrainInfo[] data, IList<int> minority) {
			switch (minority.Count) {
			case 1:
				return BuildPoint (data, minority, Edges4);
			case 2:
				return BuildLine (data, minority, Edges4, false);
			default:
				return new MeshInfo();
			}
		}

		private MeshInfo Build6SegmentMesh(PositionTerrainInfo[] data, IList<int> minority) {
			switch (minority.Count) {
			case 1:
				return BuildPoint (data, minority, Edges6);
			case 2:
				if (ExistsEdge (minority [0], minority [1], Edges6)) {
					return BuildLine (data, minority, Edges6, true);
				} else {
					var p0 = BuildPoint (data, new int[] { minority [0] }, Edges6);
					var p1 = BuildPoint (data, new int[] { minority [1] }, Edges6);
					p0.Append (p1, _positionTolerance);
					return p0;
				}
			case 3:
				if (ExistsEdge (minority [0], minority [1], Edges6) &&
				    ExistsEdge (minority [1], minority [2], Edges6) &&
				    ExistsEdge (minority [2], minority [0], Edges6)
				) {
					return BuildPlane (data, minority, Edges6);
				} else {
					return BuildCurve(data, minority, Edges6);
				}
			default:
				return new MeshInfo();
			}
		}

		private MeshInfo BuildPoint(PositionTerrainInfo[] data, IList<int> minority, Edge[] edges) {
			var adjEdges = GetAdjEdges(minority[0], edges).ToArray();
			return CreatePoly (data, adjEdges);
		}

		private MeshInfo BuildLine(PositionTerrainInfo[] data, IList<int> minority, Edge[] edges, bool ensureFirstEdge) {
			var edges0 = GetWithoutEdge(minority[0], minority[1], GetAdjEdges(minority[0], edges)).ToArray();
			var edges1 = GetWithoutEdge(minority[0], minority[1], GetAdjEdges(minority[1], edges)).ToArray();

			Edge[] allEdges;

			if (ensureFirstEdge) {
				var firstEdge = edges0.Where (x => !(edges1.Any (y => y.i2 == x.i2))).First ();
				edges0 = GetWithoutEdge (firstEdge.i1, firstEdge.i2, edges0).ToArray();

				allEdges = new Edge[] {firstEdge}.Concat(edges0).Concat (edges1).ToArray();
			} else {
				allEdges = edges0.Concat (edges1).ToArray();
			}

			return CreatePoly (data, allEdges);
		}

		private MeshInfo BuildPlane(PositionTerrainInfo[] data, IList<int> minority, Edge[] edges) {
			var edges0 = GetAdjEdges(minority[0], edges);
			edges0 = GetWithoutEdge (minority [0], minority [1], edges0);
			edges0 = GetWithoutEdge (minority [0], minority [2], edges0);
			var edges1 = GetAdjEdges(minority[1], edges);
			edges1 = GetWithoutEdge (minority [1], minority [0], edges1);
			edges1 = GetWithoutEdge (minority [1], minority [2], edges1);
			var edges2 = GetAdjEdges(minority[2], edges);
			edges2 = GetWithoutEdge (minority [2], minority [0], edges2);
			edges2 = GetWithoutEdge (minority [2], minority [1], edges2);

			var allEdges = edges0.Concat (edges1).Concat (edges2).ToArray ();
			return CreatePoly (data, allEdges);
		}

		private MeshInfo BuildCurve(PositionTerrainInfo[] data, IList<int> minority, Edge[] edges) {
			var edges0 = GetAdjEdges(minority[0], edges);
			edges0 = GetWithoutEdge (minority [0], minority [1], edges0);
			edges0 = GetWithoutEdge (minority [0], minority [2], edges0);
			var edges1 = GetAdjEdges(minority[1], edges);
			edges1 = GetWithoutEdge (minority [1], minority [0], edges1);
			edges1 = GetWithoutEdge (minority [1], minority [2], edges1);
			var edges2 = GetAdjEdges(minority[2], edges);
			edges2 = GetWithoutEdge (minority [2], minority [0], edges2);
			edges2 = GetWithoutEdge (minority [2], minority [1], edges2);

			// find center edges and group
			IEnumerable<Edge> centerEdges;
			var sideEdges = new List<IEnumerable<Edge>>();
			if (edges0.Count () == 2) {
				centerEdges = edges0;
				sideEdges.Add (edges1);
				sideEdges.Add (edges2);
			} else if (edges1.Count () == 2) {
				centerEdges = edges1;
				sideEdges.Add (edges0);
				sideEdges.Add (edges2);
			} else {
				centerEdges = edges2;
				sideEdges.Add (edges0);
				sideEdges.Add (edges1);
			}

			var mi = new MeshInfo ();

			// create center fold
			var foldEdges = sideEdges.Select(x => x.Where(y => centerEdges.Any(z => z.i2 == y.i2)));
			mi.Init ();
			foreach (var fe in foldEdges) {
				mi.Append (CreatePoly (data, centerEdges.Concat (fe)), _positionTolerance);
			}

			// create sides
			foreach (var side in sideEdges) {
				mi.Append(CreatePoly (data, side), _positionTolerance);
			}

			return mi;
		}

		private int GetTerrainValue(PositionTerrainInfo[] data, Edge edge) {
			return (data [edge.i1].terrainGroup == _terrainGroup) ? data [edge.i1].terrainValue : data [edge.i2].terrainValue;
		}

		private void SortPolyPoints(Vector3[] polyPoints, int[] tVals, Vector3 norm)
		{
			bool sthChanged = true;

			while (sthChanged)
			{
				sthChanged = false;
				for (int i = 1; i < polyPoints.Length - 1; i++)
				{
					Vector3 cross = Vector3.Cross(polyPoints[i+1] - polyPoints[0], polyPoints[i] - polyPoints[0]);
					float dot = Vector3.Dot(cross, norm);
					if (dot > 0)
					{
						var tmp = polyPoints[i];
						var tmpTVal = tVals [i];
						polyPoints[i] = polyPoints[i+1];
						tVals[i] = tVals[i+1];
						polyPoints[i+1] = tmp;
						tVals[i+1] = tmpTVal;
						sthChanged = true;
					}
				}
			}
		}

		private MeshInfo CreatePoly (PositionTerrainInfo[] data, IEnumerable<Edge> edges) {

			var points = edges.Select (e => (data [e.i1].position + data [e.i2].position) / 2.0f).ToArray();
			var tVals = edges.Select (e => GetTerrainValue (data, e)).ToArray ();
			var dir = Vector3.zero;
			foreach (var e in edges) {
				dir += (data [e.i1].terrainGroup == _terrainGroup) ? 
					data [e.i2].position - data [e.i1].position : 
					data [e.i1].position - data [e.i2].position;
			}

			SortPolyPoints (points, tVals, dir);

			var mi = new MeshInfo ();
			mi.Init ();
			var i = 0;
			foreach (var p in points) {
				mi.vertices.Add(p);
				var tvw = new Dictionary<int, int> ();
				tvw.Add (tVals[i], 1);
				mi.terrainValueWeights.Add (tvw);
				if (i > 1) {
					mi.indices.Add(0);
					mi.indices.Add(i - 1);
					mi.indices.Add(i);
				}
				i++;
			}
			return mi;
		}

		private IList<int> FindMinority(PositionTerrainInfo[] data)
		{
			List<int> solidIndices = new List<int>();
			List<int> emptyIndices = new List<int>();

			for (int h = 0; h < data.Length; h++)
			{
				if (data[h].terrainGroup == _terrainGroup)
					solidIndices.Add(h);
				else
					emptyIndices.Add(h);
			}

			if (solidIndices.Count <= emptyIndices.Count)
				return solidIndices;
			else
				return emptyIndices;
		}

		private IEnumerable<Edge> GetAdjEdges(int i, IEnumerable<Edge> edges)
		{
			var adjEdges = new List<Edge>();
			foreach (var e in edges) {
				if (e.i1 == i) {
					adjEdges.Add (new Edge (e.i1, e.i2));
				} else if (e.i2 == i) {
					adjEdges.Add (new Edge (e.i2, e.i1));
				}
			}
			return adjEdges;
		}

		private IEnumerable<Edge> GetWithoutEdge(int i1, int i2, IEnumerable<Edge> edges) 
		{
			return edges.Except(edges.Where(e => ((e.i1 == i1 && e.i2 == i2) || (e.i1 == i2 && e.i2 == i1))));
		}

		private bool ExistsEdge(int i1, int i2, IEnumerable<Edge> edges)
		{
			return edges.Any(e => ((e.i1 == i1 && e.i2 == i2) || (e.i1 == i2 && e.i2 == i1)));
		}
	}
}
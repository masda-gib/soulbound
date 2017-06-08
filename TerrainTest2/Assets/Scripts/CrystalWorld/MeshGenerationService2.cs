using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CrystalWorld {

	public class MeshGenerationService2 {

		private struct PositionTerrainInfo {
			public Vector3 position;
			public int terrainValue;

			public PositionTerrainInfo (Vector3 pos, int val) {
				position = pos;
				terrainValue = val;
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

		public MeshInfo GenerateMeshInfo (CrystalInfo crystal, CrystalCellService cellService, CrystalTerrainService terrainService) {

			List<Index3> indices3 = new List<Index3> ();
			List<PositionTerrainInfo> allData = new List<PositionTerrainInfo> ();

			indices3.Add(crystal.step);
			indices3.Add(cellService.GetNeighborStep (indices3[0], Neighbors.TOP_NORTH_EAST));
			indices3.Add(cellService.GetNeighborStep (indices3[0], Neighbors.NORTH_EAST));
			indices3.Add(cellService.GetNeighborStep (indices3[0], Neighbors.NORTH_WEST));
			indices3.Add(cellService.GetNeighborStep (indices3[0], Neighbors.TOP_NORTH_WEST));
			indices3.Add(cellService.GetNeighborStep (indices3[1], Neighbors.NORTH_WEST));
			indices3.Add(cellService.GetNeighborStep (indices3[0], Neighbors.EAST));
			indices3.Add(cellService.GetNeighborStep (indices3[0], Neighbors.TOP_SOUTH));

			var basePos = cellService.GetPositionAtStep (indices3 [0]);
			foreach (var i in indices3) {
				var pos = cellService.GetPositionAtStep (i);
				var val = terrainService.GetvalueAtPosition (pos);
				allData.Add (new PositionTerrainInfo(pos - basePos, val));
			}

			// 6-point base segment
			var currData = new PositionTerrainInfo[] { allData [0], allData [1], allData [2], allData [3], allData [4], allData [5] };
			var mi = BuildSegmentMesh (currData);
			if (!(mi.IsValid)) {
				mi.Init (0, 0, 0.1f);
			}
			// first 4-point segment
			currData = new PositionTerrainInfo[] { allData [0], allData [1], allData [2], allData [6] };
			mi.Append(BuildSegmentMesh (currData));
			// second 4-point segment
			currData = new PositionTerrainInfo[] { allData [0], allData [1], allData [4], allData [7] };
			mi.Append(BuildSegmentMesh (currData));

			// TODO: Some PointMeshes for both, 4 and 6 segments, don't generate

			return mi;
		}

		public Mesh ConvertToMesh(MeshInfo mi) {
			Mesh m = new Mesh ();

			if (mi.IsValid) {
				m.SetVertices (mi.vertices.ToList ());
				m.SetTriangles (mi.indices.ToList (), 0);
				m.RecalculateBounds ();
				m.RecalculateNormals ();
			}

			return m;
		}

		private MeshInfo BuildSegmentMesh (PositionTerrainInfo[] data) {
			var minority = FindMinority (data);

			switch (data.Length) {
			case 4:
				return Build4SegmentMesh (data, minority);
			case 6:
				return Build6SegmentMesh (data, minority);
			default:
				return new MeshInfo ();
			}
		}

		private MeshInfo Build4SegmentMesh(PositionTerrainInfo[] data, int[] minority) {
			switch (minority.Length) {
			case 1:
				return BuildPoint (data, minority, Edges4);
			case 2:
				return BuildLine (data, minority, Edges4, false);
			default:
				return new MeshInfo();
			}
		}

		private MeshInfo Build6SegmentMesh(PositionTerrainInfo[] data, int[] minority) {
			switch (minority.Length) {
			case 1:
				return BuildPoint (data, minority, Edges6);
			case 2:
				if (ExistsEdge (minority [0], minority [1], Edges6)) {
					return BuildLine (data, minority, Edges6, true);
				} else {
					var p0 = BuildPoint (data, new int[] { minority [0] }, Edges6);
					var p1 = BuildPoint (data, new int[] { minority [1] }, Edges6);
					p0.Append (p1);
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

		private MeshInfo BuildPoint(PositionTerrainInfo[] data, int[] minority, Edge[] edges) {
			var adjEdges = GetAdjEdges(minority[0], edges).ToArray();
			var points = adjEdges.Select (ae => (data [ae.i1].position + data [ae.i2].position) / 2.0f).ToArray();
			var norm = data [adjEdges [0].i2].position - data [adjEdges [0].i1].position;
			if (data [adjEdges [0].i1].terrainValue <= 0) {
				norm = -norm;
			}
			SortPolyPoints (points, norm);
			return CreatePoly (points);
		}

		private MeshInfo BuildLine(PositionTerrainInfo[] data, int[] minority, Edge[] edges, bool ensureFirstEdge) {
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

			var points = allEdges.Select (ae => (data [ae.i1].position + data [ae.i2].position) / 2.0f).ToArray();
			var norm = data [allEdges [0].i2].position - data [allEdges [0].i1].position;
			if (data [allEdges [0].i1].terrainValue <= 0) {
				norm = -norm;
			}
			SortPolyPoints (points, norm);
			return CreatePoly (points);
		}

		private MeshInfo BuildPlane(PositionTerrainInfo[] data, int[] minority, Edge[] edges) {
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

			var points = allEdges.Select (ae => (data [ae.i1].position + data [ae.i2].position) / 2.0f).ToArray();
			var norm = data [allEdges [0].i2].position - data [allEdges [0].i1].position;
			if (data [allEdges [0].i1].terrainValue <= 0) {
				norm = -norm;
			}
			SortPolyPoints (points, norm);
			return CreatePoly (points);
		}

		private MeshInfo BuildCurve(PositionTerrainInfo[] data, int[] minority, Edge[] edges) {
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
			var allEdges = edges0.Concat (edges1).Concat (edges2);
			var edgeGroups = allEdges.GroupBy (x => x.i2);
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

			// create center fold
			var foldEdges = edgeGroups.Where(x => x.Count() == 3).SelectMany(y => y as IEnumerable<Edge>).ToArray();
			var firstEdge = centerEdges.First ();
			foldEdges = new Edge[] {firstEdge}.Concat(GetWithoutEdge(firstEdge.i1, firstEdge.i2, foldEdges)).ToArray();

			var points = foldEdges.Select (ae => (data [ae.i1].position + data [ae.i2].position) / 2.0f).ToArray();
			var norm = data [firstEdge.i2].position - data [firstEdge.i1].position;
			if (data [firstEdge.i1].terrainValue <= 0) {
				norm = -norm;
			}
			SortPolyPoints (points, norm);
			var mi = CreatePoly (points);

			// create side triangles

			foreach (var side in sideEdges) {
				points = side.Select (ae => (data [ae.i1].position + data [ae.i2].position) / 2.0f).ToArray();
				norm = data [side.First().i2].position - data [side.First().i1].position;
				if (data [side.First().i1].terrainValue <= 0) {
					norm = -norm;
				}
				SortPolyPoints (points, norm);
				mi.Append(CreatePoly (points));
			}

			return mi;
		}

		private void SortPolyPoints(Vector3[] polyPoints, Vector3 norm)
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
						Vector3 tmp = polyPoints[i];
						polyPoints[i] = polyPoints[i+1];
						polyPoints[i+1] = tmp;
						sthChanged = true;
					}
				}
			}
		}

		private MeshInfo CreatePoly (Vector3[] points) {

			var mi = new MeshInfo ();
			mi.Init (points.Length, points.Length - 2, 0.1f);
			var i = 0;
			foreach (var p in points) {
				mi.vertices [i] = p;
				if (i > 1) {
					mi.indices [((i - 2) * 3)] = 0;
					mi.indices [((i - 2) * 3) + 1] = i - 1;
					mi.indices [((i - 2) * 3) + 2] = i;
				}
				i++;
			}
			return mi;
		}

		private int[] FindMinority(PositionTerrainInfo[] data)
		{
			List<int> solidIndices = new List<int>();
			List<int> emptyIndices = new List<int>();

			for (int h = 0; h < data.Length; h++)
			{
				if (data[h].terrainValue > 0) // TODO: make better test for solid Crystal
					solidIndices.Add(h);
				else
					emptyIndices.Add(h);
			}

			if (solidIndices.Count <= emptyIndices.Count)
				return solidIndices.ToArray();
			else
				return emptyIndices.ToArray();
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
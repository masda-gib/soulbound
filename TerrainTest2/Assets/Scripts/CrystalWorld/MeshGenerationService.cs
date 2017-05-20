using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public class MeshGenerationService {

		public Mesh GenerateMesh (CrystalInfo crystal, CrystalCellService cellService) {

			Mesh m = new Mesh ();

			List<Index3> indices3 = new List<Index3> ();
			List<Vector3> positions = new List<Vector3> ();

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
				positions.Add (pos - basePos);
			}

			List<int> tris0 = new List<int>(new int[] {
				0, 1, 2, 
				0, 2, 3, 
				0, 3, 4, 
				0, 4, 1, 
				5, 1, 4, 
				5, 4, 3, 
				5, 3, 2, 
				5, 2, 1 
			});

			List<int> tris1 = new List<int>(new int[] {
				0, 2, 1, 
				0, 1, 6, 
				0, 6, 2, 
				6, 1, 2 
			});

			List<int> tris2 = new List<int>(new int[] {
				0, 1, 4, 
				0, 7, 1, 
				0, 4, 7, 
				7, 4, 1 
			});

			m.SetVertices (positions);
			m.subMeshCount = 3;
			m.SetTriangles (tris0, 0);
			m.SetTriangles (tris1, 1);
			m.SetTriangles (tris2, 2);
			m.RecalculateBounds ();
			m.RecalculateNormals ();

			return m;

		}

	}
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalWorld {

	public class TerrainSegmentMeshGenerationService : BaseMeshInfoService, ICellMeshInfoGenerator {

		public TerrainSegmentMeshGenerationService ( IBlockService blockService, ITerrainService terrainService, IDistortionService distortionService)
			: base (blockService, terrainService, distortionService) {
		}

		public MeshInfo GenerateMeshInfo (CellInfo cell, Vector3 vertexOffset) {

			List<Index3> indices3 = new List<Index3> ();
			List<Vector3> positions = new List<Vector3> ();

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
				if (this.DistortionService != null) {
					pos = pos + this.DistortionService.GetDistortionAtPosition (pos);
				}
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

			var mi = new MeshInfo ();
			mi.vertices = positions;
			mi.indices = tris0.Concat(tris1).Concat(tris2).ToList();

			return mi;

		}

	}
}
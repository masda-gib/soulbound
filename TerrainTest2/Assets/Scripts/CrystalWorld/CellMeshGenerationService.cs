using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalWorld {

	public class CellMeshGenerationService : ICellMeshInfoGenerator {

		public float bias = 0.5f;

		public MeshInfo GenerateMeshInfo (CellInfo cell, IBlockService blockService, ITerrainService terrainService)
		{
			var nbs = blockService.GetAllNeighborSteps (cell.step);

			var basePos = blockService.GetPosition (cell.step);
			var nbVecs = nbs.Select (x => (blockService.GetPosition (x.Value) - basePos) * bias).ToArray();

			var indList = new List<int> ();
			// bottom
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_NORTH), GetIndex(nbs, Neighbors.BOTTOM_SOUTH_WEST), GetIndex(nbs, Neighbors.BOTTOM_SOUTH_EAST)});
			// lower triangles
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_NORTH), GetIndex(nbs, Neighbors.NORTH_EAST), GetIndex(nbs, Neighbors.NORTH_WEST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_SOUTH_EAST), GetIndex(nbs, Neighbors.SOUTH_EAST), GetIndex(nbs, Neighbors.EAST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_SOUTH_WEST), GetIndex(nbs, Neighbors.WEST), GetIndex(nbs, Neighbors.SOUTH_WEST)});
			// lower quads
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_NORTH), GetIndex(nbs, Neighbors.BOTTOM_SOUTH_EAST), GetIndex(nbs, Neighbors.NORTH_EAST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_SOUTH_EAST), GetIndex(nbs, Neighbors.EAST), GetIndex(nbs, Neighbors.NORTH_EAST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_SOUTH_EAST), GetIndex(nbs, Neighbors.BOTTOM_SOUTH_WEST), GetIndex(nbs, Neighbors.SOUTH_EAST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_SOUTH_WEST), GetIndex(nbs, Neighbors.SOUTH_WEST), GetIndex(nbs, Neighbors.SOUTH_EAST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_SOUTH_WEST), GetIndex(nbs, Neighbors.BOTTOM_NORTH), GetIndex(nbs, Neighbors.WEST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.BOTTOM_NORTH), GetIndex(nbs, Neighbors.NORTH_WEST), GetIndex(nbs, Neighbors.WEST)});
			// upper triangles
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_SOUTH), GetIndex(nbs, Neighbors.SOUTH_EAST), GetIndex(nbs, Neighbors.SOUTH_WEST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_NORTH_WEST), GetIndex(nbs, Neighbors.WEST), GetIndex(nbs, Neighbors.NORTH_WEST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_NORTH_EAST), GetIndex(nbs, Neighbors.NORTH_EAST), GetIndex(nbs, Neighbors.EAST)});
			// upper quads
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_NORTH_WEST), GetIndex(nbs, Neighbors.TOP_SOUTH), GetIndex(nbs, Neighbors.SOUTH_WEST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_NORTH_WEST), GetIndex(nbs, Neighbors.SOUTH_WEST), GetIndex(nbs, Neighbors.WEST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_SOUTH), GetIndex(nbs, Neighbors.TOP_NORTH_EAST), GetIndex(nbs, Neighbors.EAST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_SOUTH), GetIndex(nbs, Neighbors.EAST), GetIndex(nbs, Neighbors.SOUTH_EAST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_NORTH_EAST), GetIndex(nbs, Neighbors.TOP_NORTH_WEST), GetIndex(nbs, Neighbors.NORTH_WEST)});
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_NORTH_EAST), GetIndex(nbs, Neighbors.NORTH_WEST), GetIndex(nbs, Neighbors.NORTH_EAST)});
			// top
			indList.AddRange(new int[] {GetIndex(nbs, Neighbors.TOP_SOUTH), GetIndex(nbs, Neighbors.TOP_NORTH_WEST), GetIndex(nbs, Neighbors.TOP_NORTH_EAST)});

			var mi = new MeshInfo ();
			mi.vertices = nbVecs;
			mi.indices = indList.ToArray ();
			mi.positionTolerance = 0.1f * blockService.Spacing;
			return mi;
		}

		private int GetIndex(IDictionary<Neighbors, Index3> dict, Neighbors neighbor) {
			int index = -1;
			int i = 0;
			foreach (var k in dict.Keys) {
				if (k == neighbor) {
					index = i;
				}
				i++;
			}
			return index;
		}

	}

}
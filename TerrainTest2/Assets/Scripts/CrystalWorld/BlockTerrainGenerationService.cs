using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalWorld {

	public class BlockTerrainGenerationService {

		private IBlockService blockService;
		private ITerrainService terrainService;
		private TerrainMeshGenerationService cellTerrainGenerator;

		public IBlockService BlockService {
			get { return blockService; }
			set { blockService = value; }
		}

		public ITerrainService TerrainService {
			get { return terrainService; }
			set { terrainService = value; }
		}

		public IDistortionService DistortionService {
			get { return cellTerrainGenerator.distortionService; }
			set { cellTerrainGenerator.distortionService = value; }
		}

		public BlockTerrainGenerationService() {
			cellTerrainGenerator = new TerrainMeshGenerationService ();
		}

		public MeshInfo GenerateMeshInfo () {
			
			var mi = new MeshInfo();
			mi.Init (0, 0, 0.1f);

			foreach (var c in blockService) {

				var cmi = cellTerrainGenerator.GenerateMeshInfo (c, blockService, terrainService);
				if (cmi.IsValid) {
					cmi.vertices = cmi.vertices.Select (x => x + c.pos).ToArray ();
					mi.Append (cmi);
				}
			}

			return mi;
		}

	}

}

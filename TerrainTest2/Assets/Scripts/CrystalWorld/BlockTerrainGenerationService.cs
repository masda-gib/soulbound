using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalWorld {

	public class BlockTerrainGenerationService {

		private IBlockService blockService;
		private ITerrainService terrainService;
		private TerrainMeshGenerationService cellTerrainGenerator;

		private MeshInfo lowMi;
		private MeshInfo highMi;

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
			
			lowMi = new MeshInfo();
			lowMi.Init (0, 0, 0.1f);

			foreach (var c in blockService) {

				var cmi = cellTerrainGenerator.GenerateMeshInfo (c, blockService, terrainService);
				if (cmi.IsValid) {
					cmi.vertices = cmi.vertices.Select (x => x + c.pos).ToArray ();
					lowMi.Append (cmi);
				}
			}

			lowMi.GenerateNormals ();

			return lowMi;
		}

		public MeshInfo GenerateHighDetailMeshInfo() {

			var newVerts = new List<Vector3> ();
			var newInds = new List<int> ();
			var highMi = new MeshInfo ();
			highMi.positionTolerance = lowMi.positionTolerance * 0.5f;

			newVerts.AddRange (lowMi.vertices);

			for (int i = 0; i < lowMi.indices.Length / 3; i++) {
				var oi0 = lowMi.indices[i * 3];
				var oi1 = lowMi.indices[(i * 3) + 1];
				var oi2 = lowMi.indices[(i * 3) + 2];
				var ni01 = AppendVertex (newVerts, (lowMi.vertices [oi0] + lowMi.vertices [oi1]) / 2.0f, highMi.positionTolerance);
				var ni12 = AppendVertex (newVerts, (lowMi.vertices [oi1] + lowMi.vertices [oi2]) / 2.0f, highMi.positionTolerance);
				var ni20 = AppendVertex (newVerts, (lowMi.vertices [oi2] + lowMi.vertices [oi0]) / 2.0f, highMi.positionTolerance);

				newInds.AddRange (new int[] {oi0, ni01, ni20});
				newInds.AddRange (new int[] {oi1, ni12, ni01});
				newInds.AddRange (new int[] {oi2, ni20, ni12});
				newInds.AddRange (new int[] {ni01, ni12, ni20});
			}

			highMi.vertices = newVerts.ToArray ();
			highMi.indices = newInds.ToArray ();
			highMi.GenerateNormals ();

			return highMi;
		}

		public int AppendVertex(List<Vector3> verts, Vector3 newVert, float positionTolerance) {
			var vIndex = -1;
			for (int i = 0; i < verts.Count; i++) {
				if (Vector3.Distance (verts [i], newVert) <= positionTolerance) {
					vIndex = i;
				}
			}

			if (vIndex < 0) {
				verts.Add(newVert);
				return verts.Count - 1;
			} else {
				return vIndex;
			}

		}

	}

}

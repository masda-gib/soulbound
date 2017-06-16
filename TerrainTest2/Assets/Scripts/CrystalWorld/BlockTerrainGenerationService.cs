using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CrystalWorld {

	public class BlockTerrainGenerationService : IMeshInfoService {

		private TerrainMeshGenerationService _cellTerrainGenerator;

		public IBlockService BlockService {
			get { return _cellTerrainGenerator.BlockService; }
		}

		public ITerrainService TerrainService {
			get { return _cellTerrainGenerator.TerrainService; }
		}

		public IDistortionService DistortionService {
			get { return _cellTerrainGenerator.DistortionService; }
		}

		public BlockTerrainGenerationService( IBlockService blockService, ITerrainService terrainService, IDistortionService distortionService) {
			_cellTerrainGenerator = new TerrainMeshGenerationService (blockService, terrainService, distortionService);
		}

		public MeshInfo GenerateMeshInfo () {
			
			var lowMi = new MeshInfo();
			lowMi.Init ();

			foreach (var c in this.BlockService) {

				var cmi = _cellTerrainGenerator.GenerateMeshInfo (c, c.pos);
				lowMi.Append (cmi, this.BlockService.Spacing * 0.1f);
			}

			lowMi.GenerateNormals ();

			return lowMi;
		}

		public MeshInfo GenerateHighDetailMeshInfo(MeshInfo lowMi) {

			var newVerts = new List<Vector3> ();
			var newInds = new List<int> ();
			var highMi = new MeshInfo ();
			var positionTolerance = this.BlockService.Spacing * 0.05f;

			newVerts.AddRange (lowMi.vertices);

			for (int i = 0; i < lowMi.indices.Count / 3; i++) {
				var oi0 = lowMi.indices[i * 3];
				var oi1 = lowMi.indices[(i * 3) + 1];
				var oi2 = lowMi.indices[(i * 3) + 2];
				var ni01 = AppendVertex (newVerts, (lowMi.vertices [oi0] + lowMi.vertices [oi1]) / 2.0f, positionTolerance);
				var ni12 = AppendVertex (newVerts, (lowMi.vertices [oi1] + lowMi.vertices [oi2]) / 2.0f, positionTolerance);
				var ni20 = AppendVertex (newVerts, (lowMi.vertices [oi2] + lowMi.vertices [oi0]) / 2.0f, positionTolerance);

				newInds.AddRange (new int[] {oi0, ni01, ni20});
				newInds.AddRange (new int[] {oi1, ni12, ni01});
				newInds.AddRange (new int[] {oi2, ni20, ni12});
				newInds.AddRange (new int[] {ni01, ni12, ni20});
			}

			highMi.vertices = newVerts;
			highMi.indices = newInds;
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

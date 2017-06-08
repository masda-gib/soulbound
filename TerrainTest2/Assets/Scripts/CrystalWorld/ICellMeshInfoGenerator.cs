using System.Collections;
using System.Collections.Generic;

namespace CrystalWorld {

	public interface ICellMeshInfoGenerator {

	MeshInfo GenerateMeshInfo (CellInfo cell, IBlockService blockService, ITerrainService terrainService);

	}

}

using System.Collections;
using System.Collections.Generic;

namespace CrystalWorld {

	public interface ICrystalMeshInfoGenerator {

	MeshInfo GenerateMeshInfo (CrystalInfo crystal, CrystalCellService cellService, CrystalTerrainService terrainService);

	}

}

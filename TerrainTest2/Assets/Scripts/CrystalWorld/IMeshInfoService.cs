using System.Collections;
using System.Collections.Generic;

namespace CrystalWorld {

	public interface IMeshInfoService {

		IBlockService BlockService { get; }

		ITerrainService TerrainService { get; }

		IDistortionService DistortionService { get; }

	}

}

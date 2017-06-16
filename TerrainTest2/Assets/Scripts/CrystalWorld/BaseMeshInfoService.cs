using System.Collections;
using System.Collections.Generic;

namespace CrystalWorld {

	public abstract class BaseMeshInfoService : IMeshInfoService {

		private IBlockService _blockService;
		private ITerrainService _terrainService;
		private IDistortionService _distortionService;

		public BaseMeshInfoService( IBlockService blockService, ITerrainService terrainService, IDistortionService distortionService) {
			this._blockService = blockService;
			this._terrainService = terrainService;
			this._distortionService = distortionService;
		}

		#region IMeshService implementation

		public virtual IBlockService BlockService {
			get { return _blockService; }
		}

			public virtual ITerrainService TerrainService {
			get { return _terrainService; }
		}

			public virtual IDistortionService DistortionService {
			get { return _distortionService; }
		}

		#endregion

	}

}
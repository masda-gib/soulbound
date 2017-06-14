using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public interface ITerrainService {

		int GetMaterialGroup (Vector3 pos);

		int GetMaterialType (Vector3 pos, int group);

	}

}
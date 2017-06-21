using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public interface ITerrainService {

		int GetTerrainGroup (Vector3 pos);

		int GetTerrainValue (Vector3 pos, int group);

	}

}
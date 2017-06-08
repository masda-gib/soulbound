using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public interface ITerrainService {

		int GetValueAtPosition (Vector3 pos);

	}

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public interface IDistortionService {

		Vector3 GetDistortionAtPosition (Vector3 pos);

	}

}
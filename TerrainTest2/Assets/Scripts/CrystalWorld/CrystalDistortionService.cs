using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public class CrystalDistortionService : IDistortionService {

		public float maxDistance;

		#region IDistortionService implementation
		public Vector3 GetDistortionAtPosition (Vector3 pos)
		{
			var val = (pos.x + pos.y + pos.z) * 1234;
			var vec = new Vector3 (0, (val % 10) * 0.1f * maxDistance, 0);
			var quat = Quaternion.Euler ((val % 18), (val % 36), 0);
			vec = quat * vec;
			return vec;
		}
		#endregion

	}

}
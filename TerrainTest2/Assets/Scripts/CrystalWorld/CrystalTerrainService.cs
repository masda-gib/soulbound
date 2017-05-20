using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public class CrystalTerrainService {

		public float horiScale;
		public float vertScale;

		public CrystalTerrainService(float hori, float vert) {
			horiScale = hori;
			vertScale = vert;
		}

		public int GetvalueAtPosition(Vector3 pos)
		{
			var height = vertScale * (Mathf.Sin (pos.x / horiScale) + Mathf.Sin (pos.z / horiScale));
			return Mathf.RoundToInt(Mathf.Max(height - pos.y, 0));
		}
	}

}

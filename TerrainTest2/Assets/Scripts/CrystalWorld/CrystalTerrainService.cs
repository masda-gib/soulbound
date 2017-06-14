using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public class CrystalTerrainService : ITerrainService {

		public float horiScale;
		public float vertScale;

		public CrystalTerrainService(float hori, float vert) {
			horiScale = hori;
			vertScale = vert;
		}

		public int GetMaterialGroup(Vector3 pos)
		{
			var height = vertScale * (Mathf.Sin (pos.x / horiScale) + Mathf.Sin (pos.z / horiScale));
			return Mathf.RoundToInt(Mathf.Max(height - pos.y, 0));
		}

		public int GetMaterialType(Vector3 pos, int group) {
			if (group > 0) {
				return (pos.y > 0) ? 1 : 0;
			}
			return 0;
		}
	}

}

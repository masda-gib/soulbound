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

		public int GetTerrainGroup(Vector3 pos)
		{
			var height = vertScale * (Mathf.Sin (pos.x / horiScale) + Mathf.Sin (pos.z / horiScale));
			return (height - pos.y >= 0) ? 1 : 0;
		}

		public int GetTerrainValue(Vector3 pos, int group) {
			if (group > 0) {
				if (pos.y > 0) {
					return 2;
				} else {
					if (pos.x > 0) {
						return 1;
					}
					return 0;
				}
			}
			return 0;
		}
	}

}

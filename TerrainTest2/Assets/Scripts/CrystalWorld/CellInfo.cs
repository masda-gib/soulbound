﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public struct CellInfo {

		public Index3 step;
		public Vector3 pos;

		public CellInfo(Index3 index3, Vector3 position) {
			this.step = index3;
			this.pos = position;
		}
	}

}

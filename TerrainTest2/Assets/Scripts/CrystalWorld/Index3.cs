using System.Collections;
using System.Collections.Generic;

namespace CrystalWorld {

	public struct Index3 {

		public int east;
		public int up;
		public int north;

		public Index3(int east, int up, int north)
			: this()
		{
			this.east = east;
			this.up = up;
			this.north = north;
		}

		// not for crystalIndices... Create Offset3 for cell offsets to use this
//		public static Position3 operator * (Index3 ind, Position3 vec)
//		{
//			return new Position3 (ind.east * vec., ind.y * vec.y, ind.z * vec.z);
//		}

		public bool Equals (Index3 other)
		{
			return (this.east == other.east && this.up == other.up && this.north == other.north);
		}

		public override string ToString ()
		{
			return "[E:" + east + ", N:" + north + ", U:" + up + "]";
		}

	}

}

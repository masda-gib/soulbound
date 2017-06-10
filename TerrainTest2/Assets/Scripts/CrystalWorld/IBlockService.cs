using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public interface IBlockService : IEnumerable<CellInfo> {

		int EastwardSegments { get; }
		int NorthwardDoublets { get; }
		int UpwardTriplets { get; }
		float Spacing { get; }

		int CellCount { get; }

		Index3 MaxStep { get; }

		Vector3 Dimensions { get; }

		CellInfo GetCellInfo (Index3 step);

		Vector3 GetPosition (Index3 step);

		Index3 GetNeighborStep (Index3 step, Neighbors neighbor);

		IDictionary<Neighbors, Index3> GetAllNeighborSteps (Index3 step);
	}

}
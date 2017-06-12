using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public class CrystalBlockService : IBlockService {

		public const float yFactor = 0.8f;
		public const float zFactor = 0.8660254f;

		public int xSegments, ySegments, zSegments;
		public float spacing;

		public CrystalBlockService(int xSegs, int ySegs, int zSegs, float spacing) {
			this.xSegments = xSegs;
			this.ySegments = ySegs;
			this.zSegments = zSegs;
			this.spacing = spacing;
		}

		public CrystalBlockService(float spacing) : this (26, 10, 15, spacing) {
		}

		public int EastwardSegments {
			get { return xSegments; }
		}

		public int NorthwardDoublets {
			get { return zSegments; }
		}

		public int UpwardTriplets {
			get { return ySegments; }
		}

		public float Spacing {
			get { return spacing; }
		}

		public int CellCount {
			get { return xSegments * ySegments * zSegments * 6; }
		}

		public Index3 MaxStep {
			get { return new Index3 (xSegments, 3 * ySegments, 2 * zSegments); }
		}
	
		public Vector3 Dimensions {
			get { return new Vector3 (xSegments * spacing, 3 * ySegments * spacing * yFactor, 2 * zSegments * spacing * zFactor); }
		}

		public CellInfo GetCellInfo(Index3 step) {
			var pos = GetPosition (step);
			return new CellInfo (step, pos);
		}

		public Vector3 GetPosition(Index3 step)
		{
			var dims = this.Dimensions;
			float xCoord = (-0.5f * dims.x) + ((step.east + 0.5f) * spacing);
			float yCoord = (-0.5f * dims.y) + ((step.up + 0.5f) * spacing * yFactor);
			float zCoord = (-0.5f * dims.z) + ((step.north + 0.5f) * spacing * zFactor);

			if (step.north % 2 == 0) 
			{
				xCoord -= spacing / 2.0f;
			}

			switch (step.up % 3)
			{
			case 0:
				zCoord -= spacing * zFactor / 3f; // eigentlich /3, aber dann muss x-Versatz umgekehrt werden, wenn yStep%3 = 1
				break;
			case 1:
				xCoord += spacing / 2.0f;
				break;
			case 2:
				zCoord += spacing * zFactor / 3.0f;
				break;
			}

			return new Vector3(xCoord, yCoord, zCoord);
		}

		private Index3 CalcNeighborStep(Index3 step, Neighbors neighbor, int xDir, int zDirDown, int zDir, int zDirUp)
		{
			var nStep = new Index3 ();
			switch (neighbor) 
			{
			// Oben-Nord-Ost
			case Neighbors.TOP_NORTH_EAST:
				nStep.east = step.east + (xDir * zDirUp) + zDir;
				nStep.up = step.up + 1;
				nStep.north = step.north + zDirUp;
				break;

				// Oben-Nord-West
			case Neighbors.TOP_NORTH_WEST:
				nStep.east = step.east - 1 + (xDir * zDirUp) + zDir;
				nStep.up = step.up + 1 ;
				nStep.north = step.north + zDirUp;
				break;

				// Oben-Süd
			case Neighbors.TOP_SOUTH:
				nStep.east = step.east + (xDir * (1 - zDirUp)) - zDirDown;
				nStep.up = step.up + 1;
				nStep.north = step.north - 1 + zDirUp;
				break;

				// Ost
			case Neighbors.EAST:
				nStep.east = step.east + 1;
				nStep.up = step.up;
				nStep.north = step.north;
				break;

				// Nord-Ost
			case Neighbors.NORTH_EAST:
				nStep.east = step.east + xDir;
				nStep.up = step.up;
				nStep.north = step.north + 1;
				break;

				// Nord-West
			case Neighbors.NORTH_WEST:
				nStep.east = step.east - 1 + xDir;
				nStep.up = step.up;
				nStep.north = step.north + 1;
				break;

				// West
			case Neighbors.WEST:
				nStep.east = step.east - 1;
				nStep.up = step.up;
				nStep.north = step.north;
				break;

				// Süd-West
			case Neighbors.SOUTH_WEST:
				nStep.east = step.east - 1 + xDir;
				nStep.up = step.up;
				nStep.north = step.north - 1;
				break;

				// Süd-Ost
			case Neighbors.SOUTH_EAST:
				nStep.east = step.east + xDir;
				nStep.up = step.up;
				nStep.north = step.north - 1;
				break;

				// Unten-Nord
			case Neighbors.BOTTOM_NORTH:
				nStep.east = step.east - zDirUp + (xDir * (1 - zDirDown));
				nStep.up = step.up - 1;
				nStep.north = step.north + 1 - zDirDown;
				break;

				// Unten-Süd-West
			case Neighbors.BOTTOM_SOUTH_WEST:
				nStep.east = step.east - 1 + zDir + (xDir * zDirDown);
				nStep.up = step.up - 1 ;
				nStep.north = step.north - zDirDown;
				break;

				// Unten-Süd-Ost
			case Neighbors.BOTTOM_SOUTH_EAST:
				nStep.east = step.east + zDir + (xDir * zDirDown);
				nStep.up = step.up - 1;
				nStep.north = step.north - zDirDown;
				break;
			}
			return nStep;
		}

		private void CalcCorrectionValues(Index3 step, out int xDir, out int zDirDown, out int zDir, out int zDirUp)
		{
			xDir = step.north % 2;
			zDirDown = (step.up % 3 == 0) ? 1 : 0;
			zDir = (step.up % 3 == 1) ? 1 : 0;
			zDirUp = (step.up % 3 == 2) ? 1 : 0;
		}

		public Index3 GetNeighborStep(Index3 step, Neighbors neighbor)
		{
			int xDir;
			int zDirDown;
			int zDir;
			int zDirUp;
			CalcCorrectionValues (step, out xDir, out zDirDown, out zDir, out zDirUp);

			return CalcNeighborStep(step, neighbor, xDir, zDirDown, zDir, zDirUp);
		}

		public IDictionary<Neighbors, Index3> GetAllNeighborSteps(Index3 step)
		{
			var enumVals = System.Enum.GetValues (typeof(Neighbors));
			var nbs = new Dictionary<Neighbors, Index3>();

			int xDir;
			int zDirDown;
			int zDir;
			int zDirUp;
			CalcCorrectionValues (step, out xDir, out zDirDown, out zDir, out zDirUp);

			for(int i = 0; i < enumVals.Length; i++)
			{
				var nb = (Neighbors)(enumVals.GetValue(i));
				nbs.Add(nb, CalcNeighborStep(step, nb, xDir, zDirDown, zDir, zDirUp));
			}

			return nbs;
		}

		#region IEnumerable implementation

		public IEnumerator<CellInfo> GetEnumerator ()
		{
			var maxInd = MaxStep;
			for (int y = 0; y < maxInd.up; y++) 
			{
				for (int z = 0; z < maxInd.north; z++) 
				{
					for (int x = 0; x < maxInd.east; x++) 
					{
						var index3 = new Index3(x, y, z);
						yield return GetCellInfo (index3);
					}
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return this.GetEnumerator ();
		}

		#endregion

	}

}
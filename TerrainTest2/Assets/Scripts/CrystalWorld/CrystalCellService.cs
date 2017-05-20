using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrystalWorld {

	public class CrystalCellService : IEnumerable<CrystalInfo> {

		public const float yFactor = 0.8f;
		public const float zFactor = 0.8660254f;

		public int xSegments, ySegments, zSegments;
		public float spacing;

		public CrystalCellService(int xSegs, int ySegs, int zSegs, float spacing) {
			this.xSegments = xSegs;
			this.ySegments = ySegs;
			this.zSegments = zSegs;
			this.spacing = spacing;
		}

		public int Count {
			get { return xSegments * ySegments * zSegments * 6; }
		}

		public Index3 MaxStep {
			get { return new Index3 (xSegments, 3 * ySegments, 2 * zSegments); }
		}
	
		public Vector3 Dimensions {
			get { return new Vector3 (xSegments * spacing, 3 * ySegments * spacing * yFactor, 2 * zSegments * spacing * zFactor); }
		}

		public CrystalInfo GetCrystalInfoAtStep(Index3 step) {
			var pos = GetPositionAtStep (step);
			return new CrystalInfo (step, pos);
		}

		public Vector3 GetPositionAtStep(Index3 step)
		{
			var dims = this.Dimensions;
			float xCoord = (-0.5f * dims.x) + ((step.x + 0.5f) * spacing);
			float yCoord = (-0.5f * dims.y) + ((step.y + 0.5f) * spacing * yFactor);
			float zCoord = (-0.5f * dims.z) + ((step.z + 0.5f) * spacing * zFactor);

			if (step.z % 2 == 0) 
			{
				xCoord -= spacing / 2.0f;
			}

			switch (step.y % 3)
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
				nStep.x = step.x + (xDir * zDirUp) + zDir;
				nStep.y = step.y + 1;
				nStep.z = step.z + zDirUp;
				break;

				// Oben-Nord-West
			case Neighbors.TOP_NORTH_WEST:
				nStep.x = step.x - 1 + (xDir * zDirUp) + zDir;
				nStep.y = step.y + 1 ;
				nStep.z = step.z + zDirUp;
				break;

				// Oben-Süd
			case Neighbors.TOP_SOUTH:
				nStep.x = step.x + (xDir * (1 - zDirUp)) - zDirDown;
				nStep.y = step.y + 1;
				nStep.z = step.z - 1 + zDirUp;
				break;

				// Ost
			case Neighbors.EAST:
				nStep.x = step.x + 1;
				nStep.y = step.y;
				nStep.z = step.z;
				break;

				// Nord-Ost
			case Neighbors.NORTH_EAST:
				nStep.x = step.x + xDir;
				nStep.y = step.y;
				nStep.z = step.z + 1;
				break;

				// Nord-West
			case Neighbors.NORTH_WEST:
				nStep.x = step.x - 1 + xDir;
				nStep.y = step.y;
				nStep.z = step.z + 1;
				break;

				// West
			case Neighbors.WEST:
				nStep.x = step.x - 1;
				nStep.y = step.y;
				nStep.z = step.z;
				break;

				// Süd-West
			case Neighbors.SOUTH_WEST:
				nStep.x = step.x - 1 + xDir;
				nStep.y = step.y;
				nStep.z = step.z - 1;
				break;

				// Süd-Ost
			case Neighbors.SOUTH_EAST:
				nStep.x = step.x + xDir;
				nStep.y = step.y;
				nStep.z = step.z - 1;
				break;

				// Unten-Nord
			case Neighbors.BOTTOM_NORTH:
				nStep.x = step.x - zDirUp + (xDir * (1 - zDirDown));
				nStep.y = step.y - 1;
				nStep.z = step.z + 1 - zDirDown;
				break;

				// Unten-Süd-West
			case Neighbors.BOTTOM_SOUTH_WEST:
				nStep.x = step.x - 1 + zDir + (xDir * zDirDown);
				nStep.y = step.y - 1 ;
				nStep.z = step.z - zDirDown;
				break;

				// Unten-Süd-Ost
			case Neighbors.BOTTON_SOUTH_EAST:
				nStep.x = step.x + zDir + (xDir * zDirDown);
				nStep.y = step.y - 1;
				nStep.z = step.z - zDirDown;
				break;
			}
			return nStep;
		}

		private void CalcCorrectionValues(Index3 step, out int xDir, out int zDirDown, out int zDir, out int zDirUp)
		{
			xDir = step.z % 2;
			zDirDown = (step.y % 3 == 0) ? 1 : 0;
			zDir = (step.y % 3 == 1) ? 1 : 0;
			zDirUp = (step.y % 3 == 2) ? 1 : 0;
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

		public IEnumerator<CrystalInfo> GetEnumerator ()
		{
			var maxInd = MaxStep;
			for (int y = 0; y < maxInd.y; y++) 
			{
				for (int z = 0; z < maxInd.z; z++) 
				{
					for (int x = 0; x < maxInd.x; x++) 
					{
						var index3 = new Index3(x, y, z);
						yield return GetCrystalInfoAtStep (index3);
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
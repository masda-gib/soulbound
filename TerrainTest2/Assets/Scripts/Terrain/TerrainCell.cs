using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainCell : MonoBehaviour 
{
	public Index3 index;

	public TerrainWorld terrainParent;

	private void Awake()
	{
		terrainParent = GetComponentInParent<TerrainWorld> ();
		if (terrainParent != null && !terrainParent.cells.Contains(this)) 
		{
			terrainParent.cells.Add(this);
			transform.localPosition = new Vector3(
				index.x * terrainParent.CellWidth, 
				index.y * terrainParent.CellHeight, 
				index.z * terrainParent.CellLength);
		}
	}

	private void OnDestroy()
	{
		if (terrainParent != null && terrainParent.cells.Contains(this)) 
		{
			terrainParent.cells.Remove(this);
		}
	}

	public IEnumerable<Index3> AllSteps
	{
		get
		{
			for (int y = 0; y < terrainParent.cellHeightNum; y++) 
			{
				for (int z = 0; z < terrainParent.cellLengthNum; z++) 
				{
					for (int x = 0; x < terrainParent.cellWidthNum; x++) 
					{
						yield return new Index3(x, y, z);
					}
				}
			}
		}
	}

	public byte GetCrystalValue(Index3 step)
	{
		return (step.y > 5) ? (byte)0 : (byte)1;
	}

	public Vector3 GetWorldCrystalPosition(Index3 step)
	{
		var loc = GetLocalCrystalPosition (step);
		return transform.position + loc;
	}

	public Vector3 GetLocalCrystalPosition(Index3 step)
	{
		float hFac = TerrainHelpers.HeightFactor;
		float lFac = TerrainHelpers.LengthFactor;

		float xCoord = (-0.5f * terrainParent.CellWidth) + ((step.x + 0.5f) * terrainParent.baseSpacing);
		float yCoord = (-0.5f * terrainParent.CellHeight) + ((step.y + 0.5f) * terrainParent.baseSpacing * hFac);
		float zCoord = (-0.5f * terrainParent.CellLength) + ((step.z + 0.5f) * terrainParent.baseSpacing * lFac);
		
		if (step.z % 2 == 0) 
		{
			xCoord -= terrainParent.baseSpacing / 2.0f;
		}

		switch (step.y % 3)
		{
		case 0:
			zCoord -= terrainParent.baseSpacing * lFac / 3f; // eigentlich /3, aber dann muss x-Versatz umgekehrt werden, wenn yStep%3 = 1
			break;
		case 1:
			xCoord += terrainParent.baseSpacing / 2.0f;
			break;
		case 2:
			zCoord += terrainParent.baseSpacing * lFac / 3.0f;
			break;
		}

		return new Vector3(xCoord, yCoord, zCoord);
	}

	public Dictionary<Index3, Index3> GetAllNeighborStepsByCell(Index3 step)
	{
		return null;
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

	public Index3[] GetAllNeighborSteps(Index3 step)
	{
		var enumVals = System.Enum.GetValues (typeof(Neighbors));
		var nbs = new Index3[enumVals.Length];

		int xDir;
		int zDirDown;
		int zDir;
		int zDirUp;
		CalcCorrectionValues (step, out xDir, out zDirDown, out zDir, out zDirUp);

		for(int i = 0; i < enumVals.Length; i++)
		{
			var nb = (Neighbors)(enumVals.GetValue(i));
			nbs[i] = CalcNeighborStep(step, nb, xDir, zDirDown, zDir, zDirUp);
		}

		return nbs;
	}
}

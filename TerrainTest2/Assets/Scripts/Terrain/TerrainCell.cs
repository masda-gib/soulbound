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

	public Index3[] GetAllNeighborSteps(Index3 step)
	{
		List<Index3> nb = new List<Index3>();
		
		int xDir = step.z % 2;
		int zDirUp = (step.y % 3 == 2) ? 1 : 0;
		int zDir = (step.y % 3 == 1) ? 1 : 0;
		int zDirDown = (step.y % 3 == 0) ? 1 : 0;

		Index3 nSteps = new Index3();
		
		// Oben-Nord-Ost
		nSteps.x = step.x + (xDir * zDirUp) + zDir;
		nSteps.y = step.y + 1;
		nSteps.z = step.z + zDirUp;
		nb.Add(nSteps);

		// Oben-Nord-West
		nSteps.x = step.x - 1 + (xDir * zDirUp) + zDir;
		nSteps.y = step.y + 1 ;
		nSteps.z = step.z + zDirUp;
		nb.Add(nSteps);
		
		// Oben-Süd
		nSteps.x = step.x + (xDir * (1 - zDirUp)) - zDirDown;
		nSteps.y = step.y + 1;
		nSteps.z = step.z - 1 + zDirUp;
		nb.Add(nSteps);

		// Ost
		nSteps.x = step.x + 1;
		nSteps.y = step.y;
		nSteps.z = step.z;
		nb.Add(nSteps);
		
		// Nord-Ost
		nSteps.x = step.x + xDir;
		nSteps.y = step.y;
		nSteps.z = step.z + 1;
		nb.Add(nSteps);

		// Nord-West
		nSteps.x = step.x - 1 + xDir;
		nSteps.y = step.y;
		nSteps.z = step.z + 1;
		nb.Add(nSteps);

		// West
		nSteps.x = step.x - 1;
		nSteps.y = step.y;
		nSteps.z = step.z;
		nb.Add(nSteps);
		
		// Süd-West
		nSteps.x = step.x - 1 + xDir;
		nSteps.y = step.y;
		nSteps.z = step.z - 1;
		nb.Add(nSteps);

		// Süd-Ost
		nSteps.x = step.x + xDir;
		nSteps.y = step.y;
		nSteps.z = step.z - 1;
		nb.Add(nSteps);

		// Unten-Nord
		nSteps.x = step.x - zDirUp + (xDir * (1 - zDirDown));
		nSteps.y = step.y - 1;
		nSteps.z = step.z + 1 - zDirDown;
		nb.Add(nSteps);

		// Unten-Süd-West
		nSteps.x = step.x - 1 + zDir + (xDir * zDirDown);
		nSteps.y = step.y - 1 ;
		nSteps.z = step.z - zDirDown;
		nb.Add(nSteps);

		// Unten-Süd-Ost
		nSteps.x = step.x + zDir + (xDir * zDirDown);
		nSteps.y = step.y - 1;
		nSteps.z = step.z - zDirDown;
		nb.Add(nSteps);

		return nb.ToArray();
	}
}

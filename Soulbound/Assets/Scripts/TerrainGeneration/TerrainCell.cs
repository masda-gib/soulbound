using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

enum Neighbors { TopNorthEast, TopNorthWest, TopSouth, East, NorthEast, NorthWest, FarTopNorth, West, SouthWest, SouthEast };

public struct Index3
{
	public int x;
	public int y;
	public int z;
}

public class TerrainCell : MonoBehaviour 
{
	public Index3 cellIndex;
	
    private PagingTerrain mParent;

	private byte[] crystalValues;

	#region Helper methods
	internal int CalcCrystalIndex(Index3 steps)
    {
        if (
			(steps.x < 0) || (steps.x >= mParent.widthNum) ||
			(steps.y < 0) || (steps.y >= mParent.heightNum) ||
			(steps.z < 0) || (steps.z >= mParent.lengthNum)
            )
            return -1;
		return (mParent.widthNum * steps.y * mParent.lengthNum) + (mParent.widthNum * steps.z) + steps.x;
    }
	
    public Index3 CalcCrystalSteps(int index)
    {
		Index3 steps = new Index3();

        steps.y = index / (mParent.widthNum * mParent.lengthNum);
        index -= steps[1] * mParent.widthNum * mParent.lengthNum;
        steps.z = index / mParent.widthNum;
        steps.x = index - (steps[2] * mParent.widthNum);

        return steps;
    }
	
    public Vector3 CalcLocalCrystalPosition(int index)
    {
		Index3 steps = CalcCrystalSteps(index);
        return CalcLocalCrystalPosition(steps);
    }
	
	public Vector3 CalcLocalCrystalPosition(Index3 steps)
    {
        float xCoord = (-0.5f * mParent.CellWidth) + ((steps.x + 0.5f) * mParent.baseSpacing);
        float yCoord = (-0.5f * mParent.CellHeight) + ((steps.y + 0.5f) * mParent.baseSpacing * 0.8f);
        float zCoord = (-0.5f * mParent.CellLength) + ((steps.z + 0.5f) * mParent.baseSpacing * 0.8660254f);

        if (steps.z % 2 == 0)
            xCoord += mParent.baseSpacing / 4.0f;
        else
            xCoord -= mParent.baseSpacing / 4.0f;

        switch (steps.y % 3)
        {
            case 0:
                zCoord -= mParent.baseSpacing * 0.8660254f / 1.5f; // eigentlich /3, aber dann muss x-Versatz umgekehrt werden, wenn yStep%3 = 1
                break;
            case 2:
                zCoord += mParent.baseSpacing * 0.8660254f / 1.5f; // eigentlich /3, aber dann muss x-Versatz umgekehrt werden, wenn yStep%3 = 1
                break;
        }

        return new Vector3(xCoord, yCoord, zCoord);
    }

	public Index3[] GetAllNeighborCrystalSteps(Index3 steps)
    {
		List<Index3> nb = new List<Index3>();

        int xDir = steps.z % 2;  // Versatz-Vorhandensein
		int zDir = (steps.y % 3 == 2) ? 2 : 0;

		Index3 nSteps = new Index3();

        // Oben-Nord-Ost
        nSteps.x = steps.x + 1 - xDir;
        nSteps.y = steps.y + 1;
        nSteps.z = steps.z - 1 + zDir;
        nb.Add(nSteps);

        // Oben-Nord-West
        nSteps.x = steps.x - xDir;
        nSteps.y = steps.y + 1 ;
        nSteps.z = steps.z - 1 + zDir;
		nb.Add(nSteps);

        // Oben-Süd
        nSteps.x = steps.x;
        nSteps.y = steps.y + 1;
        nSteps.z = steps.z + zDir;
		nb.Add(nSteps);

        // Ost
        nSteps.x = steps.x + 1;
        nSteps.y = steps.y;
        nSteps.z = steps.z;
		nb.Add(nSteps);

        // Nord-Ost
        nSteps.x = steps.x + 1 - xDir;
        nSteps.y = steps.y;
        nSteps.z = steps.z - 1;
		nb.Add(nSteps);

        // Nord-West
        nSteps.x = steps.x - xDir;
        nSteps.y = steps.y;
        nSteps.z = steps.z - 1;
		nb.Add(nSteps);

		/*
        // Oben-Weit-Nord
        nSteps.x = steps.x;
        nSteps.y = steps.y + 1;
        nSteps.z = steps.z - 2 + zDir;
		nb.Add(nSteps);
		*/

        // West
        nSteps.x = steps.x - 1;
        nSteps.y = steps.y;
        nSteps.z = steps.z;
		nb.Add(nSteps);

        // Süd-West
        nSteps.x = steps.x - xDir;
        nSteps.y = steps.y;
        nSteps.z = steps.z + 1;
		nb.Add(nSteps);

        // Süd-Ost
        nSteps.x = steps.x + 1 - xDir;
        nSteps.y = steps.y;
        nSteps.z = steps.z + 1;
		nb.Add(nSteps);
		
		// Unten-Nord
		nSteps.x = steps.x + 1 - xDir;
		nSteps.y = steps.y - 1;
		nSteps.z = steps.z + zDir;
		nb.Add(nSteps);
		
		// Unten-Süd-West
		nSteps.x = steps.x - xDir;
		nSteps.y = steps.y - 1 ;
		nSteps.z = steps.z + 1 + zDir;
		nb.Add(nSteps);
		
		// Unten-Süd-Ost
		nSteps.x = steps.x;
		nSteps.y = steps.y - 1;
		nSteps.z = steps.z + 1 + zDir;
		nb.Add(nSteps);

        return nb.ToArray();
    }
	#endregion

	public byte GetCrystalValue(int index)
	{
		return crystalValues[index];
	}

	public byte GetCrystalValue(Index3 steps)
	{
		return crystalValues[CalcCrystalIndex(steps)];
	}
	
	public void UnLoad()
	{
		Destroy(this.gameObject);
	}

	// Use this for initialization
	void Start () 
	{
		if(mParent == null)
        	mParent = transform.parent.gameObject.GetComponent<PagingTerrain>();
		
        crystalValues = new byte[mParent.widthNum * mParent.heightNum * mParent.lengthNum];
	}
}

using UnityEngine;
using System.Collections;

public class TerrainData 
{
	public TerrainGenerator terrainGenerator;
	public string saveDataFolder;
	
	// cell geometric specifications
	// good values for spacing [10, 8.660254, 8]:
	// [26, 30, 30] or multiples. This makes the cells slightly flat
	public int widthNum = 26;
	public int heightNum = 30;
	public int lengthNum = 30;
	public float baseSpacing = 1.5f;
	
	// calculated cell size
	private Vector3 cellDimensions;

	private bool initialized = false;

	#region Properties
	public Vector3 CellDimensions
	{
		get 
		{ 
			if (!initialized)
				CalculateCellDimensions ();
			return cellDimensions; 
		}
	}
	#endregion

	// Use this for initialization
	public TerrainData () 
	{
		CalculateCellDimensions ();
		initialized = true;
	}

	private void CalculateCellDimensions ()
	{
		cellDimensions.x = widthNum * baseSpacing;
		cellDimensions.y = heightNum * baseSpacing * 0.8f;
		cellDimensions.z = lengthNum * baseSpacing * 0.8660254f;
	}

	public Vector3 GetCellIndex(Vector3 position)
	{
		Vector3 dims = CellDimensions;
		int x = Mathf.FloorToInt((position.x / dims.x) + 0.5f);
		int y = Mathf.FloorToInt((position.y / dims.y) + 0.5f);
		int z = Mathf.FloorToInt((position.z / dims.z) + 0.5f);
		return new Vector3(x, y, z);
	}
	
	public Vector3 GetCellCenter(Vector3 index)
	{
		Vector3 dims = CellDimensions;
		return new Vector3(index.x * dims.x, index.y * dims.y, index.z * dims.z);
	}

	public Bounds GetCellBounds(Vector3 index)
	{
		Vector3 ctr = GetCellCenter(index);
		return new Bounds(ctr, CellDimensions);
	}
}

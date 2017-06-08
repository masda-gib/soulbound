//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//public class TerrainWorld : MonoBehaviour 
//{
//	// cell geometric specifications
//	// good values for spacing [10, 8.660254, 8]:
//	// [26, 30, 30] or multiples. This makes the cells slightly flat
//	// y has to be a multiple of 3
//	// z has to be a multiple of 2
//	public int cellWidthNum = 26;
//	public int cellHeightNum = 30;
//	public int cellLengthNum = 30;
//	public float baseSpacing = 1.5f;
//
//	public List<TerrainCell> cells;
//
//	public float CellWidth
//	{
//		get { return cellWidthNum * baseSpacing; }
//	}
//	public float CellHeight
//	{
//		get { return cellHeightNum * baseSpacing * TerrainHelpers.HeightFactor; }
//	}
//	public float CellLength
//	{
//		get { return cellLengthNum* baseSpacing * TerrainHelpers.LengthFactor; }
//	}
//}

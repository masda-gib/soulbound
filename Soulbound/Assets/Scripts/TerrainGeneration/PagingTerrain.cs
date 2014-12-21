using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PagingTerrain : MonoBehaviour 
{
    #region fields
    // terrain properties
    public string lvlName;

    public TerrainGenerator terrainGenerator;

    // the 'camera' that runs across the terrain
    public Transform cameraTransform;
	public GameObject cellPrefab;

	// cell geometric specifications
    // good values for spacing [10, 8.660254, 8]:
    // [26, 30, 30] or multiples. This makes the cells slightly flat
    public int widthNum = 26;
    public int heightNum = 30;
    public int lengthNum = 30;
    public float baseSpacing = 1.5f;

	// Cache configuration
    public int cellCacheSize = 100;
    public int cacheRadius = 3;
    public int drawRadius = 1;

	// calculated cell size
	private float cellWidth;
    private float cellHeight;
    private float cellLength;

    // cache and caching/drawing radi and offset vectors
    private CellCache cellCache;
    private Vector3[] cacheNeighbors, drawNeighbors;

    // runtime variables
    private Vector3 currCellIndex;
    private bool currCellInvalid;
    private TerrainCell drawCell;
    #endregion

	#region Properties
    public float CellWidth
    {
        get { return cellWidth; }
    }
    public float CellHeight
    {
        get { return cellHeight; }
    }
    public float CellLength
    {
        get { return cellLength; }
    }
	#endregion
	public void SetInvalid()
	{
		currCellInvalid = true;
	}
	
    public Vector3 GetCellIndex(Vector3 position)
    {
        int x = Mathf.FloorToInt((position.x / cellWidth) + 0.5f);
        int y = Mathf.FloorToInt((position.y / cellHeight) + 0.5f);
        int z = Mathf.FloorToInt((position.z / cellLength) + 0.5f);
        return new Vector3(x, y, z);
    }
	
    public Vector3 GetCellCenter(Vector3 index)
    {
        return new Vector3(index.x * CellWidth, index.y * CellHeight, index.z * CellLength);
    }
	
    public Bounds /*Vector3[]*/ GetCellBounds(Vector3 index)
    {
        Vector3 ctr = GetCellCenter(index);
//		Vector3 bMin = new Vector3(ctr.x - CellWidth / 2.0f, ctr.y - CellHeight / 2.0f, ctr.z- CellLength / 2.0f);
//		Vector3 bMax = new Vector3(ctr.x + CellWidth / 2.0f, ctr.y + CellHeight / 2.0f, ctr.z+ CellLength / 2.0f);
//		return new Vector3[] { bMin, bMax };
		return new Bounds(ctr, new Vector3(cellWidth, cellHeight, cellLength));
    }

	void Awake ()
	{
       	terrainGenerator = new TestGenerator();

		cellCache = new CellCache(cellCacheSize);
		
        //crystalGen = new CrystalGenerator(gen.Generate);

        cellWidth = widthNum * baseSpacing;
        cellHeight = heightNum * baseSpacing * 0.8f;
        cellLength = lengthNum * baseSpacing * 0.8660254f;

        List<Vector3> cacheList = new List<Vector3>();
        List<Vector3> drawList = new List<Vector3>();

		for (int x = -cacheRadius; x <= cacheRadius; x++)
        {
            for (int z = -cacheRadius; z <= cacheRadius; z++)
            {
				for (int y = -cacheRadius; y <= cacheRadius; y++)
				{
	                Vector3 vec = new Vector3(x, y, z);
	                if (vec.magnitude <= ((float)cacheRadius + 0.5f))
	                {
	                    cacheList.Add(vec);
	                }
	                if (vec.magnitude <= ((float)drawRadius + 0.5f))
	                {
	                    drawList.Add(vec);
	                }
				}
            }
        }

		cacheNeighbors = cacheList.ToArray();
        drawNeighbors = drawList.ToArray();
		
		SetInvalid();
	}
	
	// Update is called once per frame
	void Update () {
        if (cameraTransform == null)
            return;

        Vector3 camCell = GetCellIndex(cameraTransform.position);

        if ((currCellInvalid) || (camCell != currCellIndex))
        {
            currCellIndex = camCell;
            foreach (Vector3 n in cacheNeighbors)
            {
                Vector3 testCellIndex = currCellIndex + n;
				GameObject cellObject;
                TerrainCell cellContent = (TerrainCell)cellCache.Get(testCellIndex);
                if (cellContent == null)
                {
                    // load new cell
					cellObject = (GameObject)GameObject.Instantiate(cellPrefab, GetCellCenter(testCellIndex), Quaternion.identity);
					cellObject.transform.parent = this.transform;
                    cellContent = cellObject.GetComponent<TerrainCell>();
					cellContent.cellIndex = testCellIndex;
                    cellCache.Set(testCellIndex, cellContent);
                    //cellContent.Load("test", testCellIndex, crystalGen);
					// TODO: Set vars for loading here, loading should then start in Start() of cell;
					
                }
				else
				{
					cellObject = cellContent.gameObject;
				}
				
            }
			
			for (int i=0; i < transform.childCount; i++)
			{
				bool drawThis = false;
				Transform child = transform.GetChild(i);
				foreach(Vector3 d in drawNeighbors)
				{
					if(GetCellIndex(child.position) == currCellIndex+d)
						drawThis = true;
				}
				if(drawThis && !child.gameObject.activeSelf)
					child.gameObject.SetActive(true);
				if(!drawThis && child.gameObject.activeSelf)
					child.gameObject.SetActive(false);
			}

			currCellInvalid = false;
            Debug.Log(cellCache.Count);
        }
	}
}

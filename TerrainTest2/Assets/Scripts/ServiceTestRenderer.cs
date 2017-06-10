using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CrystalWorld;

public class ServiceTestRenderer : MonoBehaviour 
{
	public int lengthSegs = 2;
	public int widthSegs = 4;
	public int heightSegs = 1;
	public int spacing = 1;
	public float distortion = 0.5f;

	public float horiScale = 8;
	public float vertScale = 8;

	public Index3 debugIndex;

	private CrystalBlockService _cellService;
	private CrystalTerrainService _terrainService;

	private List<GameObject> objs;

	public CrystalBlockService CellService {
		get { return _cellService; }
	}

	public CrystalTerrainService TerrainService {
		get { return _terrainService; }
	}

	void Awake() {
		_cellService = new CrystalBlockService (widthSegs, heightSegs, lengthSegs, spacing);
		_terrainService = new CrystalTerrainService (horiScale, vertScale);
	}

	// Use this for initialization
	void Start () {

		// Debug
		objs = new List<GameObject> ();
		objs.Add (GameObject.CreatePrimitive (PrimitiveType.Cube));
		int nbs = System.Enum.GetNames (typeof(Neighbors)).Length;
		for (int i = 0; i < nbs; i++) {
			objs.Add (GameObject.CreatePrimitive (PrimitiveType.Sphere));
		}
		foreach (var o in objs) {
			o.transform.localScale = Vector3.one * spacing * 1.2f;
		}

//		// Terrain
		foreach (var c in _cellService) {
			var tVal = _terrainService.GetValueAtPosition (c.pos);
			if (tVal > 0) {
				var obj = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				obj.transform.parent = this.transform;
				obj.transform.position = new Vector3 (c.pos.x, c.pos.y, c.pos.z);
				obj.transform.localScale = Vector3.one * spacing;
			}
		}
	}

	public void Update()
	{
		objs [0].transform.position = _cellService.GetPosition (debugIndex);
		int i = 1;
		foreach (var s in _cellService.GetAllNeighborSteps (debugIndex)) 
		{
			objs[i].transform.position = _cellService.GetPosition(s.Value);
			i++;
		}
	}
}

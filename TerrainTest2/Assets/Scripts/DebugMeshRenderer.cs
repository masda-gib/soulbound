using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrystalWorld;

public class DebugMeshRenderer : MonoBehaviour {

	public ServiceTestRenderer sr;
	public bool generateTerrain;
	public Material[] mats;
	public Index3 debugIndex;

	private MeshGenerationService mgs1;
	private MeshGenerationService2 mgs2;
	private Index3 _lastIndex;
	private GameObject renderGo;
	private MeshFilter renderMf;

	// Use this for initialization
	void Start () {
		mgs1 = new MeshGenerationService ();
		mgs2 = new MeshGenerationService2 ();
		renderGo = new GameObject ("DebugRenderer");
		renderMf = renderGo.AddComponent<MeshFilter> ();
		var renderMr = renderGo.AddComponent<MeshRenderer> ();
		renderMr.materials = mats;
		renderGo.transform.parent = this.transform;

		if (generateTerrain) {
			foreach (var c in sr.CellService) {
				var terrainGo = new GameObject ("TerrainRenderer");
				var terrainMf = terrainGo.AddComponent<MeshFilter> ();
				terrainGo.transform.parent = this.transform;
				terrainGo.transform.position = c.pos;
				terrainMf.mesh = mgs2.GenerateMesh (c, sr.CellService, sr.TerrainService);
				var terrainMr = terrainGo.AddComponent<MeshRenderer> ();
				terrainMr.materials = mats;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (_lastIndex.x != debugIndex.x || _lastIndex.y != debugIndex.y || _lastIndex.z != debugIndex.z) {
			_lastIndex = debugIndex;

			var c = sr.CellService.GetCrystalInfoAtStep (_lastIndex);
			Mesh debugMesh = mgs1.GenerateMesh (c, sr.CellService, sr.TerrainService);
			renderMf.mesh = debugMesh;
			renderGo.transform.position = sr.CellService.GetPositionAtStep (_lastIndex);
		}
		
	}

}

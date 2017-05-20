using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrystalWorld;

public class DebugMeshRenderer : MonoBehaviour {

	public ServiceTestRenderer sr;
	public bool generateTerrain;
	public Material[] mats;
	public Index3 debugIndex;

	private MeshGenerationService mgs;
	private Index3 _lastIndex;
	private GameObject renderGo;
	private MeshFilter renderMf;

	// Use this for initialization
	void Start () {
		mgs = new MeshGenerationService ();
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
				terrainMf.mesh = mgs.GenerateMesh (c, sr.CellService);
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
			Mesh debugMesh = mgs.GenerateMesh (c, sr.CellService);
			renderMf.mesh = debugMesh;
			renderGo.transform.position = sr.CellService.GetPositionAtStep (_lastIndex);
		}
		
	}

}

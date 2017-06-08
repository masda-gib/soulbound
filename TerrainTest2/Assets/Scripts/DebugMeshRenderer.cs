using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CrystalWorld;

public class DebugMeshRenderer : MonoBehaviour {

	public ServiceTestRenderer sr;
	public bool generateTerrain;
	public bool debugTerrain;
	public Material mat;
	public Index3 debugIndex;

	private MeshGenerationService mgs1;
	private MeshGenerationService2 mgs2;
	private Index3 _lastIndex;
	private GameObject renderGo;
	private MeshFilter renderMf;
	private MeshRenderer renderMr;

	// Use this for initialization
	void Start () {
		mgs1 = new MeshGenerationService ();
		mgs2 = new MeshGenerationService2 ();
		renderGo = new GameObject ("DebugRenderer");
		renderMf = renderGo.AddComponent<MeshFilter> ();
		renderMr = renderGo.AddComponent<MeshRenderer> ();
		renderGo.transform.parent = this.transform;

		if (debugTerrain) {
			foreach (var c in sr.CellService) {
				if (sr.TerrainService.GetvalueAtPosition (c.pos) > 0) {
					var terrainGo = new GameObject ("DebugTerrain");
					var terrainMf = terrainGo.AddComponent<MeshFilter> ();
					terrainGo.transform.parent = this.transform;
					terrainGo.transform.position = c.pos;
					terrainMf.mesh = mgs1.GenerateMesh (c, sr.CellService, sr.TerrainService);
					var terrainMr = terrainGo.AddComponent<MeshRenderer> ();
					terrainMr.materials = new Material[terrainMf.mesh.subMeshCount];
					for (int i = 0; i < terrainMr.materials.Length; i++) {
						terrainMr.materials [i] = mat;
					}
				}
			}
		}

		if (generateTerrain) {
			var terrainGo = new GameObject ("MeshTerrain");
			var terrainMf = terrainGo.AddComponent<MeshFilter> ();
			terrainGo.transform.parent = this.transform;
			terrainGo.transform.position = this.transform.position;
			var mi = new MeshInfo();
			mi.Init (0, 0, 0.1f);

			foreach (var c in sr.CellService) {

				var cmi = mgs2.GenerateMeshInfo (c, sr.CellService, sr.TerrainService);
				if (cmi.IsValid) {
					cmi.vertices = cmi.vertices.Select (x => x + c.pos).ToArray ();
					mi.Append (cmi);
				}
			}

			terrainMf.mesh = mgs2.ConvertToMesh (mi);
			var terrainMr = terrainGo.AddComponent<MeshRenderer> ();
			terrainMr.material = mat;
		}
	}

	// Update is called once per frame
	void Update () {

		if (_lastIndex.x != debugIndex.x || _lastIndex.y != debugIndex.y || _lastIndex.z != debugIndex.z) {
			_lastIndex = debugIndex;

			var c = sr.CellService.GetCrystalInfoAtStep (_lastIndex);
			Mesh debugMesh = mgs1.GenerateMesh (c, sr.CellService, sr.TerrainService);
			renderMf.mesh = debugMesh;
			renderMr.materials = new Material[renderMf.mesh.subMeshCount];
			for (int i = 0; i < renderMr.materials.Length; i++) {
				renderMr.materials [i] = mat;
			}
			renderGo.transform.position = sr.CellService.GetPositionAtStep (_lastIndex);
			renderGo.name = "Debug: " + c.step.x.ToString() + ", " + c.step.y.ToString() + ", " + c.step.z.ToString();
		}
		
	}

}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CrystalWorld;

public class DebugMeshRenderer : MonoBehaviour {

	public ServiceTestRenderer sr;
	public bool generateTerrain;
	public int terrainGroup;
	public float smooth;
	public float relax;
	public bool hdTerrain;
	public bool debugTerrain;
	public Material mat;
	public float distortion;
	public int debugEast;
	public int debugNorth;
	public int debugUp;
	public float debugBias = 0.5f;

	private CellMeshGenerationService mgs1;
	private BlockTerrainGenerationService mgs2;
	private ICellMeshInfoGenerator mgs3;
	private Index3 _lastIndex;
	private GameObject renderGo;
	private MeshFilter renderMf;

	// Use this for initialization
	void Start () {
		var ds = new CrystalDistortionService ();
		ds.maxDistance = sr.CellService.Spacing * distortion;
		mgs1 = new CellMeshGenerationService (sr.CellService, sr.TerrainService, ds);
		mgs2 = new BlockTerrainGenerationService (sr.CellService, sr.TerrainService, ds);
		mgs3 = new TerrainSegmentMeshGenerationService (sr.CellService, sr.TerrainService, ds);

		renderGo = new GameObject ("DebugRenderer");
		renderMf = renderGo.AddComponent<MeshFilter> ();
		var renderMr = renderGo.AddComponent<MeshRenderer> ();
		renderMr.material = mat;
		renderGo.transform.parent = this.transform;

		if (debugTerrain) {
			foreach (var c in sr.CellService) {
				if (sr.TerrainService.GetTerrainGroup (c.pos) > 0) {
					var terrainGo = new GameObject ("DebugTerrain");
					var terrainMf = terrainGo.AddComponent<MeshFilter> ();
					terrainGo.transform.parent = this.transform;
					terrainGo.transform.position = c.pos;

					var mi = mgs1.GenerateMeshInfo (c, Vector3.zero);
					terrainMf.mesh = ConvertToMesh(mi, true);
					var terrainMr = terrainGo.AddComponent<MeshRenderer> ();
					terrainMr.material = mat;
				}
			}
		}

		if (generateTerrain) {
			var terrainGo = new GameObject ("MeshTerrain");
			var terrainMf = terrainGo.AddComponent<MeshFilter> ();
			terrainGo.transform.parent = this.transform;
			terrainGo.transform.position = this.transform.position;

			var mi = mgs2.GenerateMeshInfo (terrainGroup);
			mi.Smooth (smooth, relax);
			mi.GenerateNormals ();
			if (hdTerrain) {
				mi = mgs2.GenerateHighDetailMeshInfo (mi);
			}
			mi.ClearCache ();

			terrainMf.mesh = ConvertToMesh (mi, false);
			var terrainMr = terrainGo.AddComponent<MeshRenderer> ();
			terrainMr.material = mat;
		}
	}

	// Update is called once per frame
	void Update () {

		if (_lastIndex.east != debugEast || _lastIndex.up != debugUp || _lastIndex.north != debugNorth) {
			_lastIndex = new Index3(debugEast, debugUp, debugNorth);

			var c = sr.CellService.GetCellInfo (_lastIndex);

			mgs1.bias = debugBias;
			var mi = mgs3.GenerateMeshInfo (c, Vector3.zero);
			renderMf.mesh = ConvertToMesh (mi, true);
			renderGo.transform.position = sr.CellService.GetPosition (_lastIndex);
			renderGo.name = "Debug: " + c.step.ToString();
		}
		
	}

	protected Mesh ConvertToMesh(MeshInfo mi, bool reCalculateNormals) {
		Mesh m = new Mesh ();

		if (mi.IsValid) {
			m.SetVertices (mi.vertices);
			if (mi.terrainValueWeights != null) {
				m.SetColors (mi.terrainValueWeights.Select (x => GetVertexColor (x)).ToList ());
			}
			m.SetTriangles (mi.indices, 0);
			m.RecalculateBounds ();
			if (reCalculateNormals) {
				m.RecalculateNormals ();
			} else {
				m.SetNormals(mi.normals.ToList());
			}
		}

		return m;
	}

	private Color32 GetVertexColor (IDictionary<int, int> tVals) {
		if (tVals.Count > 0) {
			var topVal = tVals.OrderByDescending (x => x.Value).First().Key;
			switch (topVal) {
			case 1:
				return Color.yellow;
			case 2:
				return Color.grey;
			default:
				return Color.green;
			}
		}
		return Color.black;
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrystalWorld;

public class DebugMeshRenderer : MonoBehaviour {

	public ServiceTestRenderer sr;
	public Index3 debugIndex;

	private Index3 _lastIndex;
	private GameObject renderGo;
	private MeshFilter renderMf;

	// Use this for initialization
	void Start () {
		renderGo = new GameObject ("DebugRenderer");
		renderMf = renderGo.AddComponent<MeshFilter> ();
		var r = renderGo.AddComponent<MeshRenderer> ();
		r.enabled = true;
		renderGo.transform.parent = this.transform;
	}
	
	// Update is called once per frame
	void Update () {

		if (_lastIndex.x != debugIndex.x || _lastIndex.y != debugIndex.y || _lastIndex.z != debugIndex.z) {
			_lastIndex = debugIndex;

			Mesh debugMesh = CreateMesh (_lastIndex, sr.CellService);
			renderMf.mesh = debugMesh;
			renderGo.transform.position = sr.CellService.GetPositionAtStep (_lastIndex);
		}
		
	}

	private Mesh CreateMesh(Index3 index3, CrystalCellService service) {
		Mesh m = new Mesh ();

		List<Index3> indices3 = new List<Index3> ();
		List<Vector3> positions = new List<Vector3> ();

		indices3.Add(index3);
		indices3.Add(service.GetNeighborStep (indices3[0], Neighbors.TOP_NORTH_EAST));
		indices3.Add(service.GetNeighborStep (indices3[0], Neighbors.NORTH_EAST));
		indices3.Add(service.GetNeighborStep (indices3[0], Neighbors.NORTH_WEST));
		indices3.Add(service.GetNeighborStep (indices3[0], Neighbors.TOP_NORTH_WEST));
		indices3.Add(service.GetNeighborStep (indices3[1], Neighbors.NORTH_WEST));

		var basePos = service.GetPositionAtStep (indices3 [0]);
		foreach (var i in indices3) {
			var pos = service.GetPositionAtStep (i);
			positions.Add (pos - basePos);
		}

		List<int> tris = new List<int>(new int[] {
			0, 1, 2, 
			0, 2, 3, 
			0, 3, 4, 
			0, 4, 1, 
			5, 1, 4, 
			5, 4, 3, 
			5, 3, 2, 
			5, 2, 1 
		});

		m.SetVertices (positions);
		m.SetTriangles (tris, 0);
		m.RecalculateBounds ();
		m.RecalculateNormals ();

		return m;
	}

}

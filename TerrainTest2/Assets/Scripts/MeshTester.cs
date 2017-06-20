using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTester : MonoBehaviour {

	public Color[] colors;
	public Color32[] colors32;

	// Use this for initialization
	void Start () {

		var mf = GetComponent<MeshFilter> ();

		colors = mf.mesh.colors;
		colors32 = mf.mesh.colors32;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestRenderer : MonoBehaviour 
{
	public TerrainCell cell;
	public Index3 index;
	public float vertScale;
	public float horiScale;
	public bool optimize;
	public float crystalSize;

	private Dictionary<Index3, bool> values;
	private List<GameObject> objs;

	public bool GetPointValueAtPosition(Vector3 pos)
	{
		var height = vertScale * (Mathf.Sin (pos.x / horiScale) + Mathf.Sin (pos.z / horiScale));
		return pos.y < height;
	}

	public IEnumerator Start()
	{
		objs = new List<GameObject> ();
		objs.Add (GameObject.CreatePrimitive (PrimitiveType.Cube));
		int nbs = cell.GetAllNeighborSteps (index).Length;
		for(int i = 0; i < nbs; i++) 
		{
			objs.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
		}
		foreach(var go in objs)
		{
			go.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
		}

		values = new Dictionary<Index3, bool> ();
		Debug.Log ("Calculating values");
		foreach (var s in cell.AllSteps) 
		{
			var wPos = cell.GetWorldCrystalPosition(s);
			var val = GetPointValueAtPosition(wPos);
			values.Add(s, val);
		}
		Debug.Log ("Creating objects");
		yield return new WaitForFixedUpdate();
		foreach (var s in cell.AllSteps) 
		{
			if(values[s])
			{
				var draw = !optimize;
				if(!draw)
				{
					var snbs = cell.GetAllNeighborSteps(s);
					foreach(var snb in snbs)
					{
						bool val;
						if(values.TryGetValue(snb, out val) && !val)
						{
							draw = true;
							break;
						}
					}
				}

				if(draw)
				{
					yield return new WaitForFixedUpdate();
					var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					go.transform.position = cell.GetWorldCrystalPosition(s);
					go.transform.localScale = Vector3.one * crystalSize;
				}
			}
		}
		Debug.Log ("Done!");
	}

	public void Update()
	{
		objs [0].transform.position = cell.GetWorldCrystalPosition (index);
		int i = 1;
		foreach (var s in cell.GetAllNeighborSteps (index)) 
		{
			objs[i].transform.position = cell.GetWorldCrystalPosition(s);
			i++;
		}
	}
}

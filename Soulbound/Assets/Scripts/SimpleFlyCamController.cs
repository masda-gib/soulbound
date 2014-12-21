using UnityEngine;
using System.Collections;

public class SimpleFlyCamController : MonoBehaviour 
{
	private float mouseX;
	private float mouseY;
	private float forward;
	private float sideward;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		mouseX = Input.GetAxis("Mouse X");
		mouseY = Input.GetAxis("Mouse Y");
		forward = Input.GetAxis("Vertical");
		sideward = Input.GetAxis ("Horizontal");
		
		transform.Rotate(Vector3.up, mouseX * Time.deltaTime);
		transform.Rotate(transform.right, -mouseY * Time.deltaTime);
		transform.position += transform.forward * 10.0f * forward * Time.deltaTime;
		transform.position += transform.right * 10.0f * sideward * Time.deltaTime;
	}
}

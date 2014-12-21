using UnityEngine;
using System.Collections;

public class PersonController : MonoBehaviour, IPerson, ICharacterController
{
	public float speed = 30;
	public float forwadBias = 0.5f;
	public float jumpPower = 10;
	public Vector2 turnSpeed = new Vector2(180, 360);
	public float maxVerticalAngle = 80;

	public Transform head;
	public Transform primaryUseSlot;
	public Transform secondaryUseSlot;
    
	private Vector2 movementSpeedVector;
	private bool shouldJump;
	private Vector2 lookVector;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		DoMove();
		DoLook();
		DoJump();
	}

	private void DoMove()
	{
		float dot = Vector2.Dot (Vector2.up, movementSpeedVector);
		float biasInf = (1 - dot) * 0.5f;
		float mag = movementSpeedVector.magnitude;
		float ratio = Mathf.Max(1, mag);
		float squash = 1 - (forwadBias * biasInf);
		
		Vector2 m = movementSpeedVector * squash / ratio;
		m *= Time.deltaTime * speed;
		
		rigidbody.MovePosition (transform.position + transform.forward * m.y + transform.right * m.x);
	}

	private void DoLook()
	{
		float turnAmount = Mathf.Clamp(lookVector.x, -turnSpeed.x * Time.deltaTime, turnSpeed.x * Time.deltaTime);

		float xRot = head.localRotation.eulerAngles.x;
		if (xRot > 180)
		{
			xRot -= 360;
		}
		float lookAmount = Mathf.Clamp(-lookVector.y, -turnSpeed.y * Time.deltaTime, turnSpeed.y * Time.deltaTime);
		xRot = Mathf.Clamp(xRot + lookAmount, -maxVerticalAngle, maxVerticalAngle);

		rigidbody.MoveRotation(Quaternion.AngleAxis(turnAmount, transform.up) * transform.rotation);
		head.localRotation = Quaternion.Euler(xRot, 0, 0);
    }

	public void ResetActions()
	{
		lookVector = Vector2.zero;
		movementSpeedVector = Vector2.zero;
		shouldJump = false;
    }

	private void DoJump()
	{
		if (shouldJump) 
		{
			rigidbody.AddForce(transform.up * jumpPower, ForceMode.Impulse);
			shouldJump = false;
		}
	}

	#region ICharacterController implementation

	public void MoveForward (float amount)
	{
		if (enabled) 
		{
			movementSpeedVector.y = amount;
		}
	}

	public void MoveToSide(float amount)
	{
		if (enabled) 
		{
			movementSpeedVector.x = amount;
		}
	}		

	public void EnterLeave ()
	{
		throw new System.NotImplementedException ();
	}

	public void SelectUp ()
	{
		throw new System.NotImplementedException ();
	}

	public void SelectDown ()
	{
		throw new System.NotImplementedException ();
	}
	
	public void Look (Vector2 dir)
	{
		if (enabled) 
		{
			lookVector = dir;
		}
	}

	#endregion

	#region IActionController implementation

	public void BeginJump ()
	{
		shouldJump = enabled;
	}

	public void ContinueJump (bool doContinue)
	{
		throw new System.NotImplementedException ();
	}

	public void BeginDuck ()
	{
		throw new System.NotImplementedException ();
	}

	public void ContinueDuck (bool doContinue)
	{
		throw new System.NotImplementedException ();
	}

	public void BeginUse ()
	{
		throw new System.NotImplementedException ();
	}

	public void ContinueUse (bool doContinue)
	{
		throw new System.NotImplementedException ();
	}

	#endregion
}

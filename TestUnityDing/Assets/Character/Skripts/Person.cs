using UnityEngine;
using System.Collections;

public class Person : MonoBehaviour, IPerson, ICharacterController
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

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		DoMove();
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
		
		GetComponent<Rigidbody>().MovePosition (transform.position + transform.forward * m.y + transform.right * m.x);
	}

	#region IPerson implementation

	public IPersonContainer CurrentContainer 
	{
		get;
		set;
	}

	#endregion

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
        if (CurrentContainer == null) 
        {
            var conts = GameObject.FindObjectsOfType<Garment> ();
            foreach (var c in conts) {
                if (Vector3.Distance (this.transform.position, c.transform.position) < 10) 
                {
                    c.RequestPilotEntering (this);
                    break;
                }
            }
        } else 
        {
            CurrentContainer.EjectPilot();
        }
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
            float turnAmount = Mathf.Clamp(dir.x, -turnSpeed.x * Time.deltaTime, turnSpeed.x * Time.deltaTime);
            
            float xRot = head.localRotation.eulerAngles.x;
            if (xRot > 180)
            {
                xRot -= 360;
            }
            float lookAmount = Mathf.Clamp(-dir.y, -turnSpeed.y * Time.deltaTime, turnSpeed.y * Time.deltaTime);
            xRot = Mathf.Clamp(xRot + lookAmount, -maxVerticalAngle, maxVerticalAngle);
            
            GetComponent<Rigidbody>().MoveRotation(Quaternion.AngleAxis(turnAmount, transform.up) * transform.rotation);
            head.localRotation = Quaternion.Euler(xRot, 0, 0);
		}
	}
    
    #endregion
    
    #region IActionController implementation

	public void BeginJump ()
	{
		if (enabled) 
		{
		    GetComponent<Rigidbody>().AddForce (transform.up * jumpPower, ForceMode.Impulse);
		}
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

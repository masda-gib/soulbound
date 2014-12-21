using UnityEngine;
using System.Collections;

public class DefaultCharacterInput : MonoBehaviour 
{
    public Person person;
	public ICharacterController character;

	public float forwardAmount;
	public float sideAmount;
	public Vector2 look;

	// Use this for initialization
	void Start () 
	{
	    if (character == null) 
        {
            character = person;
        }
	}

	// Update is called once per frame
	void Update () 
	{
		forwardAmount = Input.GetAxis ("MoveVertical");
		sideAmount = Input.GetAxis ("MoveHorizontal");

		character.MoveForward (forwardAmount);
		character.MoveToSide (sideAmount);

		if (Input.GetButtonDown("Jump"))
		{
			character.BeginJump();
		}

        if (Input.GetButtonDown("EnterLeave"))
        {
            character.EnterLeave();
        }

		look = new Vector2(Input.GetAxis("LookHorizontal"), Input.GetAxis("LookVertical"));
		character.Look(look * 10);
	}
}

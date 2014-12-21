using UnityEngine;
using System.Collections;

public abstract class AbstractCharacterController : MonoBehaviour 
{
	public abstract void BeginJump();
	public abstract void ContinueJump(bool doContinue);

	public abstract void BeginDuck();
	public abstract void ContinueDuck(bool doContinue);

	public abstract void BeginUse();
	public abstract void ContinueUse(bool doContinue);

	public abstract void EnterLeave();

	public abstract void Look (Vector2 dirChange);
	
	public abstract void MoveForward (float amount);
	public abstract void MoveToSide(float amount);

	public abstract void SelectUp();
	public abstract void SelectDown();
}

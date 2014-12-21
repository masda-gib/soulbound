using UnityEngine;
using System.Collections;

public interface ICharacterController : IActionController 
{
	void EnterLeave();
	
	void Look (Vector2 dirChange);
	
	void MoveForward (float amount);
	void MoveToSide(float amount);
	
	void SelectUp();
	void SelectDown();
}

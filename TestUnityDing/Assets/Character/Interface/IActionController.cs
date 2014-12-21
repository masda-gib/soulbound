using UnityEngine;
using System.Collections;

public interface IActionController
{
	void BeginJump();
	void ContinueJump(bool doContinue);
	
	void BeginDuck();
	void ContinueDuck(bool doContinue);
	
	void BeginUse();
	void ContinueUse(bool doContinue);
}

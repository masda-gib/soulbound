using UnityEngine;
using System.Collections;

public abstract class AbstractEquipmentController : MonoBehaviour, IEquipment
{
	public ICharacterController Owner { get; set; }

	public abstract void BeginJump();
	public abstract void ContinueJump(bool doContinue);
	
	public abstract void BeginDuck();
	public abstract void ContinueDuck(bool doContinue);
	
	public abstract void BeginUse();
	public abstract void ContinueUse(bool doContinue);

	public abstract void Select(bool state);
}

using UnityEngine;
using System.Collections;

public interface IEquipment : IActionController
{
	ICharacterController Owner { get; set; }

	void Select(bool state);
}

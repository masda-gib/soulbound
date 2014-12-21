using UnityEngine;
using System.Collections;

public interface IPersonContainer
{
	ICharacterController Controller { get; }
	IPerson CurrentPilot { get; }

	bool CanBeEnterdBy( IPerson pilot );
	bool LetPilotIn(IPerson container);
	bool EjectPilot();
}

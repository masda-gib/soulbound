using UnityEngine;
using System.Collections;

public interface IPersonContainer
{
	IPerson CurrentPilot { get; }

	//bool CanBeEnterdBy( IPerson pilot );
	bool RequestPilotEntering(IPerson pilot);
	bool EjectPilot();
}

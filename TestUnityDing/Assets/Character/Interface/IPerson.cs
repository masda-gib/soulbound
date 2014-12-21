using UnityEngine;
using System.Collections;

public interface IPerson
{
	IPersonContainer CurrentContainer { get; set; }

	//IEquipmentHolder EquippedItems { get; }
}

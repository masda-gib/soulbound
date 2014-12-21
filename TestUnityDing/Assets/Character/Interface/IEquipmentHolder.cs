using UnityEngine;
using System.Collections;

public interface IEquipmentHolder
{
	IEquipment[] Items { get; }
	int DefaultItemIndex { get; }

	void SelectItem(int i);
}

using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Index3
{
	public int x;
	public int y;
	public int z;

	public Index3(int x, int y, int z)
	: this()
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static Vector3 operator * (Index3 ind, Vector3 vec)
	{
		return new Vector3 (ind.x * vec.x, ind.y * vec.y, ind.z * vec.z);
	}

	public override string ToString ()
	{
		return "[" + x + ", " + y + ", " + z + "]";
	}
}

public static class TerrainHelpers
{
	public const float HeightFactor = 0.8f;
	public const float LengthFactor = 0.8660254f;
}

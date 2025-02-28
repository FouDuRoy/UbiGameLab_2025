using System;
using UnityEngine;


public class MathExtension
{
	public static Vector4 toHomogeneousCords(Vector3 v)
	{
		return new Vector4(v.x, v.y, v.z, 1);
	}
    public static Vector3 toEuclidianVector(Vector4 v)
    {
        return new Vector3(v.x, v.y, v.z);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    public class Vector3Uint
    {
        public uint x, y, z;

        public Vector3Uint()
        {
            
        }
    }

    public static Vector2Int MulByFloat(this Vector2Int v, float value)
    {
        v.x = (int)(v.x * value);
        v.y = (int)(v.y * value);
        return v;
    }
}

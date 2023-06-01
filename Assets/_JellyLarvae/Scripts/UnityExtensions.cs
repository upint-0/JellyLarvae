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

    public static float Remap( this float value, float old_min, float old_max, float new_min, float new_max )
    {
        float x = new_min + ( value - old_min ) * ( new_max - new_min ) / ( old_max - old_min );
        return Mathf.Clamp( x, new_min, new_max );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityExtensions
{
    public struct Vector3Uint
    {
        public uint x, y, z;
        
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

    public static Vector3 toVec3(this Vector2 vec, float z = 0f)
    {
        return new Vector3(vec.x, vec.y, z);
    }

    public static RenderTexture CloneRenderTexture(this RenderTexture sourceTexture)
    {
        RenderTexture cloneTexture = new RenderTexture(sourceTexture.width, sourceTexture.height, sourceTexture.depth,
            sourceTexture.format);

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cloneTexture;

        Graphics.Blit(sourceTexture, cloneTexture);

        RenderTexture.active = currentRT;

        return cloneTexture;
    }

}

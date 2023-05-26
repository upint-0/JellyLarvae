using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyRenderer : MonoBehaviour
{
    [SerializeField] private Vector2 _MapSize = Vector2.one;

    [Header("Compute")] [SerializeField] 
    private ComputeShader _ComputeShaderJellyMask;
    private Material _JellyRenderer;
    
    [Header("Texture settings")]
    [SerializeField] [Range(0.1f,10f)]private float _TextureQuality = 1f;
    [SerializeField] private RenderTexture _JellyMask;
    
    
    
}

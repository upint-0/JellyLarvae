using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyRenderer : MonoBehaviour
{
    [SerializeField] private Vector2Int _MapSize = Vector2Int.one;

    [Header("Compute")] 
    [SerializeField] private ComputeShader _ComputeShaderJellyMask;
    [SerializeField] private Material _JellyRenderer;
    
    [Header("Texture settings")]
    [SerializeField] [Range(0.1f,10f)]private float _TextureQuality = 1f;
    [SerializeField] private RenderTexture _JellyMask;
    
    public void SetupRenderTexture()
    {
        Vector2Int size = new Vector2Int((int) (_MapSize.x * _TextureQuality), (int) (_MapSize.y * _TextureQuality));
        _JellyMask.width = size.x;
        _JellyMask.height = size.y;
    }
    
}

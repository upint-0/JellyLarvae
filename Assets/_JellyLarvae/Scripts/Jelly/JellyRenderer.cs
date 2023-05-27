using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using static UnityExtensions;

public class JellyRenderer : MonoBehaviour
{
    [SerializeField] private Vector2Int _MapSize = Vector2Int.one;

    [Header("Compute")] 
    [SerializeField] private ComputeShader _ComputeShaderJellyMask;
    [SerializeField] private SpriteRenderer _JellyRenderer;
    private Material _MaterialInstance;
    private int _MainKernel;
    private Vector3Uint _KernelGroupeSize = new Vector3Uint();
    
    [Header("Texture settings")]
    [SerializeField] [Range(0.1f,10f)]private float _TextureQuality = 1f;
    [SerializeField] private RenderTexture _JellyMask;

    private MaterialPropertyBlock _MaterialPropertyBlock;
    
    [Button("Setup Render Texture")]
    void SetupRenderTexture()
    {
        Vector2Int size = new Vector2Int((int) (_MapSize.x * _TextureQuality), (int) (_MapSize.y * _TextureQuality));
        _JellyMask.width = size.x;
        _JellyMask.height = size.y;
    }

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        _JellyMask.Release();
    }

    private void Init()
    {
        Vector2Int size = new Vector2Int((int) (_MapSize.x * _TextureQuality), (int) (_MapSize.y * _TextureQuality));

        _JellyMask = new RenderTexture(size.x, size.y, 16, RenderTextureFormat.ARGB32);
        //_JellyMask.Create();
        _JellyMask.enableRandomWrite = true;
        
        _MaterialPropertyBlock = new MaterialPropertyBlock();
        _MaterialPropertyBlock.SetTexture("_MainTex", _JellyMask);
        
        _JellyRenderer.SetPropertyBlock(_MaterialPropertyBlock);

        


        _MainKernel = _ComputeShaderJellyMask.FindKernel("CSMain");
        _ComputeShaderJellyMask.GetKernelThreadGroupSizes(_MainKernel, out _KernelGroupeSize.x,out _KernelGroupeSize.y, out _KernelGroupeSize.z);
        
    }

    private void Update()
    {
        Draw();
        _ComputeShaderJellyMask.SetTexture(_MainKernel,"_JellyMask", _JellyMask);
        _ComputeShaderJellyMask.Dispatch(_MainKernel, (int) _KernelGroupeSize.x, (int) _KernelGroupeSize.y,(int) _KernelGroupeSize.z);
    }

    private void Draw()
    {
    }
}

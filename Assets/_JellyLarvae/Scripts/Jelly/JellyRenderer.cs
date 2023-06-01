using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityExtensions;

public class JellyRenderer : MonoBehaviour
{
    #region Variables
    [SerializeField] private Vector2Int _MapSize = Vector2Int.one;

    [Header("Paint")] 
    [SerializeField] private float _BrushSize = 1f;
    
    [Header("Compute")] 
    [SerializeField] private ComputeShader _ComputeShaderJellyMask;
    [SerializeField] private SpriteRenderer _JellyRenderer;
    
    [Header("Texture settings")]
    [SerializeField] [Range(0.1f,10f)]private float _TextureQuality = 1f;
    [SerializeField] private RenderTexture _JellyMask;

    [Header("Read value")]
    public bool _ReadValueAtMousePosition = false;
    public TextMeshProUGUI  _JellyValueDebug;

    private PointInfos _PlayerPosInfos = new PointInfos();
    private PlayerInfos _PlayerInfos = new PlayerInfos();
    private MaterialPropertyBlock _MaterialPropertyBlock;
    
    private int _MainKernel;
    private Vector3Uint _KernelGroupeSize = new Vector3Uint();

    private ComputeBuffer _PointsInfosBuffer;
    private ComputeBuffer _PlayerInfosBuffer;
    
    #endregion

    #region Consts
    private const int POINT_INFOS_SIZEOF = sizeof(float);
    private const int PLAYER_INFOS_SIZEOF = (sizeof(float) * 4) + sizeof(Single) + sizeof(float);
    #endregion
    #region Struct
    [Serializable]
    public struct PointInfos
    {
        public float jellyValue;
    }
    [System.Serializable]
    public struct PlayerInfos
    {
        public Vector2 position;
        public System.Single isEating;
        public float eatRadius;
        public Vector2 eatPosition;
    }
    #endregion

    
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
        _PointsInfosBuffer.Release();
        _PlayerInfosBuffer.Release();
    }

    private void Init()
    {
        // Canvas
        Vector2 spriteSize = _JellyRenderer.transform.localScale;

        _MapSize.x = (int)spriteSize.x;
        _MapSize.y = (int)spriteSize.y;
        
        _MapSize = _MapSize.MulByFloat(_TextureQuality);
        Vector2Int size = new Vector2Int((int) (_MapSize.x), (int) (_MapSize.y ));

        _JellyMask = new RenderTexture(size.x, size.y, 16, RenderTextureFormat.ARGB32);
        //_JellyMask.Create();
        _JellyMask.enableRandomWrite = true;
        
        _MaterialPropertyBlock = new MaterialPropertyBlock();
        _MaterialPropertyBlock.SetTexture("_MainTex", _JellyMask);
        
        _JellyRenderer.SetPropertyBlock(_MaterialPropertyBlock);

        _PointsInfosBuffer = new ComputeBuffer(1, POINT_INFOS_SIZEOF);
        _PlayerInfosBuffer = new ComputeBuffer(1, PLAYER_INFOS_SIZEOF);

        _MainKernel = _ComputeShaderJellyMask.FindKernel("CSMain");
        _ComputeShaderJellyMask.GetKernelThreadGroupSizes(_MainKernel, out _KernelGroupeSize.x,out _KernelGroupeSize.y, out _KernelGroupeSize.z);
        
        _ComputeShaderJellyMask.SetVector("_TextureSize", new Vector2(size.x, size.y));
    }

    private void Update()
    {
        // Draw with mouse
        Vector2 mousePos = Input.mousePosition;
        Vector2 mousePosWS = Camera.main.ScreenToWorldPoint(mousePos);
        
        Draw(mousePosWS);
        
        _ComputeShaderJellyMask.SetBuffer(_MainKernel, "_PlayerInfos", _PlayerInfosBuffer);
        _ComputeShaderJellyMask.SetBuffer(_MainKernel, "_PointsInfos", _PointsInfosBuffer);
        
        _ComputeShaderJellyMask.SetTexture(_MainKernel,"_JellyMask", _JellyMask);
        _ComputeShaderJellyMask.Dispatch(_MainKernel, 
            (int) (_MapSize.x / _KernelGroupeSize.x), 
            (int) (_MapSize.y /_KernelGroupeSize.y),
            (int) _KernelGroupeSize.z);
        
        _MaterialPropertyBlock.SetTexture("_MainTex", _JellyMask);

        if ( _ReadValueAtMousePosition )
        {
            GetJellyValueAtPosition(mousePosWS, false, mousePosWS, 5f);
            _JellyValueDebug.text = "Jelly Value : " + _PlayerPosInfos.jellyValue;
        } 
    }

    public float GetJellyValueAtPosition(Vector2 position, bool eat, Vector2 mouthPos, float eatRadius)
    {
        _ComputeShaderJellyMask.SetVector("playerPos", position);
        Vector3 eatSettings = new Vector3(Convert.ToSingle(eat), mouthPos.x, mouthPos.y);
        _ComputeShaderJellyMask.SetVector("playerPos", position);
        
        _PlayerInfos.position = position;
        _PlayerInfos.isEating = Convert.ToSingle(eat);
        _PlayerInfos.eatRadius = eatRadius;
        _PlayerInfos.eatPosition = mouthPos;

        PlayerInfos[] infos = new PlayerInfos[1];
        infos[0] = _PlayerInfos;
        _PlayerInfosBuffer.SetData(infos);
        
        // Read buffer data 
        PointInfos[] data = new PointInfos[1];
        _PointsInfosBuffer.GetData(data);
        _PlayerPosInfos = data[0];

        return _PlayerPosInfos.jellyValue;
    }

    private void Draw(Vector2 mousePosWS)
    {
        // Canvas
        Vector2 spriteSize = _JellyRenderer.transform.localScale;

        Vector4 canvasSettings = new Vector4(_JellyRenderer.transform.position.x, _JellyRenderer.transform.position.y,
            spriteSize.x, spriteSize.y);
        
        _ComputeShaderJellyMask.SetVector("_CanvasSettings", canvasSettings);
        



        bool isDrawing = Input.GetKey(KeyCode.Space);
        Vector4 brushSettings = new Vector4(mousePosWS.x, mousePosWS.y, _BrushSize, Convert.ToSingle(isDrawing));
        
        _ComputeShaderJellyMask.SetVector("_BrushSettings", brushSettings);
    }
}

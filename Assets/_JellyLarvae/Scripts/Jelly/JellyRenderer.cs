using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using NaughtyAttributes;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityExtensions;

public class JellyRenderer : MonoBehaviour
{
    #region Variables
    [SerializeField] private Vector2Int _MapSize = Vector2Int.one;

    [Header("Paint")]
    [SerializeField] private bool _EditorMode = false;
    [SerializeField] private float _BrushSize = 1f;
    
    [Header("Compute")] 
    [SerializeField] private ComputeShader _ComputeShaderJellyMask;
    [SerializeField] private SpriteRenderer _JellyRenderer;
    
    [Header("Texture settings")]
    [SerializeField] [Range(0.1f,10f)]private float _TextureQuality = 1f;
    [SerializeField] private Texture2D _MapSource;
    
    [Header("Save settings")]
    [SerializeField] private string _TextureMaskPath;
    [SerializeField] private string _TextureFileName = "Map_Mask";
    [SerializeField] private bool _NamingUseHorodatage = true;
    [SerializeField] private KeyCode _SaveKeyCode = KeyCode.S;


    [Header("Debug")]
    public bool _ReadValueAtMousePosition = false;
    public TextMeshProUGUI  _JellyValueDebug;
    [SerializeField,ShowAssetPreview()]
    private RenderTexture _JellyMaskInstance;

    [SerializeField] private bool _Tick = true;
    
    
    private PointInfos _PlayerPosInfos = new PointInfos();
    private PlayerInfos _PlayerInfos = new PlayerInfos();
    private MaterialPropertyBlock _MaterialPropertyBlock;
    
    private int _InitKernel;
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

    #region Init
    private void OnEnable()
    {
        Init();
    }
    private void Init()
    {
        // Canvas
        Vector2 spriteSize = _JellyRenderer.transform.localScale;

        _MapSize.x = (int)spriteSize.x;
        _MapSize.y = (int)spriteSize.y;
        
        _MapSize = _MapSize.MulByFloat(_TextureQuality);
        Vector2Int size = new Vector2Int((int) (_MapSize.x), (int) (_MapSize.y ));
        
        _JellyMaskInstance = new RenderTexture(size.x, size.y, 16, RenderTextureFormat.ARGB32);
        
        _JellyMaskInstance.enableRandomWrite = true;
        
        _MaterialPropertyBlock = new MaterialPropertyBlock();
        _MaterialPropertyBlock.SetTexture("_MainTex", _JellyMaskInstance);
        
        _JellyRenderer.SetPropertyBlock(_MaterialPropertyBlock);

        _PointsInfosBuffer = new ComputeBuffer(1, POINT_INFOS_SIZEOF);
        _PlayerInfosBuffer = new ComputeBuffer(1, PLAYER_INFOS_SIZEOF);

        _InitKernel = _ComputeShaderJellyMask.FindKernel("CSInit");
        _MainKernel = _ComputeShaderJellyMask.FindKernel("CSMain");
        _ComputeShaderJellyMask.GetKernelThreadGroupSizes(_MainKernel, out _KernelGroupeSize.x,out _KernelGroupeSize.y, out _KernelGroupeSize.z);
        
        _ComputeShaderJellyMask.SetVector("_TextureSize", new Vector2(size.x, size.y));

        CopySourceMap();
    }
    #endregion
    #region Unint

    [Button()]
    public void CopySourceMap()
    {
        if (_MapSource)
        {
            Debug.Log("Init map with texture source");
            _ComputeShaderJellyMask.SetTexture(_InitKernel, "_JellyMask", _JellyMaskInstance);
            _ComputeShaderJellyMask.SetTexture(_InitKernel, "_JellyMaskSource", _MapSource);

            _ComputeShaderJellyMask.Dispatch(_InitKernel,
                (int)(_MapSize.x / _KernelGroupeSize.x),
                (int)(_MapSize.y / _KernelGroupeSize.y),
                (int)_KernelGroupeSize.z);
        }
    }
    [Button()]
    public void SaveMap()
    {
        string path = Application.dataPath + _TextureMaskPath;
        string name = _TextureFileName + SceneManager.GetActiveScene().name;
        if (_NamingUseHorodatage) name += "_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss");
            
        _JellyMaskInstance.ExportToTexture2D(path, name,TextureUtils.Channel.R);
            
        Debug.Log("Save map !! Export to : " + path + name);
    }
    private void OnDisable()
    {
        _JellyMaskInstance.Release();
        _PointsInfosBuffer.Release();
        _PlayerInfosBuffer.Release();
    }
    #endregion

    #region Update
    private void Update()
    {
        if (!_Tick) return;
        // Draw with mouse
        Vector2 mousePos = Input.mousePosition;
        Vector2 mousePosWS = Camera.main.ScreenToWorldPoint(mousePos);
        
        Draw(mousePosWS);
        
        _ComputeShaderJellyMask.SetBuffer(_MainKernel, "_PlayerInfos", _PlayerInfosBuffer);
        _ComputeShaderJellyMask.SetBuffer(_MainKernel, "_PointsInfos", _PointsInfosBuffer);
        
        _ComputeShaderJellyMask.SetTexture(_MainKernel,"_JellyMask", _JellyMaskInstance);
        _ComputeShaderJellyMask.Dispatch(_MainKernel, 
            (int) (_MapSize.x / _KernelGroupeSize.x), 
            (int) (_MapSize.y /_KernelGroupeSize.y),
            (int) _KernelGroupeSize.z);
        
        _MaterialPropertyBlock.SetTexture("_MainTex", _JellyMaskInstance);
        _JellyRenderer.SetPropertyBlock(_MaterialPropertyBlock);
        
        if ( _ReadValueAtMousePosition )
        {
            GetJellyValueAtPosition(mousePosWS, false, mousePosWS, 5f);
            _JellyValueDebug.text = "Jelly Value : " + _PlayerPosInfos.jellyValue;
        }

        if (_EditorMode)
        {
            if(Input.GetKeyDown(_SaveKeyCode)) SaveMap();
        }
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
    #endregion
    
    #region public Methods
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
    
    #endregion


    
}

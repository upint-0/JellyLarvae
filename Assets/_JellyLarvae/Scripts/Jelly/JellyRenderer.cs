using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Obstacles")] 
    [SerializeField] private ObstacleMaskGenerator _ObstacleMaskGenerator;
    [SerializeField] private LayerMask _ObstacleLayer;
    [SerializeField, ShowAssetPreview()] private RenderTexture _ObstacleMaskPreview;
    
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
    [SerializeField,ShowAssetPreview()]
    private RenderTexture _JellyMaskInstance;

    [Space] [SerializeField] private JellySurfaceDetection _JellySurfaceDetection;

    private PointInfos _PlayerPosInfos = new PointInfos();
    private JellyReader _jellyReader = new JellyReader();
    private MaterialPropertyBlock _MaterialPropertyBlock;
    
    private int _InitKernel;
    private int _MainKernel;
    private int _RemoveJellyByMaskKernel;
    private int _RAndWJellyKernel;
    private Vector3Uint _MainKernelGroupeSize = new Vector3Uint();
    private uint _ReaderKernelGroupSize;

    private ComputeBuffer _PointsInfosBuffer;
    private ComputeBuffer _JellyReaderOutBuffer;
    private ComputeBuffer _PlayerInfosBuffer;
    private ComputeBuffer _JellyReaderBuffer;
    
    #endregion
    #region Consts
    private const int POINT_INFOS_SIZEOF = sizeof(float) + sizeof(int);
    private const int PLAYER_INFOS_SIZEOF = (sizeof(float) * 4) + sizeof(Single) + sizeof(float)+ sizeof(int);
    #endregion
    #region Struct
    [Serializable]
    public struct PointInfos
    {
        public int id;
        public float jellyValue;
    }
    [System.Serializable]
    public struct JellyReader
    {
        public int id;
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

        int maxJellyReaderCount = _JellySurfaceDetection._MaximumReader;
        
        _JellyReaderOutBuffer = new ComputeBuffer(maxJellyReaderCount, POINT_INFOS_SIZEOF);
        PointInfos[] pointInfosData = new PointInfos[maxJellyReaderCount];
        for (int i = 0; i < pointInfosData.Length; i++)
        {
            pointInfosData[i].id = i;
            pointInfosData[i].jellyValue = i * 5f;
        }
        _JellyReaderOutBuffer.SetData(pointInfosData);
        
        
        _PlayerInfosBuffer = new ComputeBuffer(1, PLAYER_INFOS_SIZEOF);
        _JellyReaderBuffer = new ComputeBuffer(maxJellyReaderCount, PLAYER_INFOS_SIZEOF);

        // Find kernel
        _InitKernel = _ComputeShaderJellyMask.FindKernel("CSInit");
        _MainKernel = _ComputeShaderJellyMask.FindKernel("CSMain");
        _RemoveJellyByMaskKernel = _ComputeShaderJellyMask.FindKernel("CSRemoveJellyByMask");
        _RAndWJellyKernel = _ComputeShaderJellyMask.FindKernel("CSReadAndWriteJelly");
        
        _ComputeShaderJellyMask.GetKernelThreadGroupSizes(_MainKernel, out _MainKernelGroupeSize.x,out _MainKernelGroupeSize.y, out _MainKernelGroupeSize.z);
        _ComputeShaderJellyMask.GetKernelThreadGroupSizes(_RAndWJellyKernel, out _ReaderKernelGroupSize,out _, out _);
        
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
                (int)(_MapSize.x / _MainKernelGroupeSize.x),
                (int)(_MapSize.y / _MainKernelGroupeSize.y),
                (int)_MainKernelGroupeSize.z);
        }
    }

    public void SaveMap(bool overrideSourceMap)
    {
        string path = Application.dataPath + _TextureMaskPath;
        string name = _TextureFileName + SceneManager.GetActiveScene().name;
        if (_NamingUseHorodatage) name += "_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss");
        if (overrideSourceMap) name = _MapSource.name + "_Obstacle";
        _JellyMaskInstance.ExportToTexture2D(path, name,TextureUtils.Channel.R);
            
        Debug.Log("Save map !! Export to : " + path + name);
    }
    private void OnDisable()
    {
        _JellyMaskInstance.Release();
        _PointsInfosBuffer.Release();
        _PlayerInfosBuffer.Release();
        _JellyReaderBuffer.Release();
        _JellyReaderOutBuffer.Release();
    }
    #endregion

    #region Update
    private void Update()
    {
        // Draw with mouse
        Vector2 mousePos = Input.mousePosition;
        Vector2 mousePosWS = Camera.main.ScreenToWorldPoint(mousePos);
        
        Draw(mousePosWS);
        
        _ComputeShaderJellyMask.SetBuffer(_MainKernel, "_PlayerInfos", _PlayerInfosBuffer);
        _ComputeShaderJellyMask.SetBuffer(_MainKernel, "_PointsInfos", _PointsInfosBuffer);
        
        _ComputeShaderJellyMask.SetTexture(_MainKernel,"_JellyMask", _JellyMaskInstance);
        _ComputeShaderJellyMask.Dispatch(_MainKernel, 
            (int) (_MapSize.x / _MainKernelGroupeSize.x), 
            (int) (_MapSize.y /_MainKernelGroupeSize.y),
            (int) _MainKernelGroupeSize.z);
        
        _MaterialPropertyBlock.SetTexture("_MainTex", _JellyMaskInstance);
        _JellyRenderer.SetPropertyBlock(_MaterialPropertyBlock);
        

        if (_EditorMode)
        {
            if(Input.GetKeyDown(_SaveKeyCode)) SaveMap(false);
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
        _jellyReader.position = position;
        _jellyReader.isEating = Convert.ToSingle(eat);
        _jellyReader.eatRadius = eatRadius;
        _jellyReader.eatPosition = mouthPos;

        JellyReader[] infos = new JellyReader[1];
        infos[0] = _jellyReader;
        _PlayerInfosBuffer.SetData(infos);
        
        // Read buffer data 
        PointInfos[] data = new PointInfos[1];
        _PointsInfosBuffer.GetData(data);
        _PlayerPosInfos = data[0];

        return _PlayerPosInfos.jellyValue;
    }

    public PointInfos[] GetJellyValue(ref Dictionary<int, JellyReader> jellyReaders)
    {
        int count = jellyReaders.Count;
        
        _JellyReaderBuffer.SetCounterValue((uint) count + 1);
        _JellyReaderBuffer.SetData(jellyReaders.Values.ToArray());

        _JellyReaderOutBuffer.SetCounterValue((uint) count);
        
        _ComputeShaderJellyMask.SetInt("_JellyReadersCount", count);
        _ComputeShaderJellyMask.SetBuffer(_RAndWJellyKernel, "_JellyReaders", _JellyReaderBuffer);
        _ComputeShaderJellyMask.SetBuffer(_RAndWJellyKernel, "_JellyReadersOut", _JellyReaderOutBuffer);
        _ComputeShaderJellyMask.SetTexture(_RAndWJellyKernel,"_JellyMask", _JellyMaskInstance);

        int dispatchSize = (int)(count / _ReaderKernelGroupSize) + 1;
        _ComputeShaderJellyMask.Dispatch(_RAndWJellyKernel, dispatchSize, 1, 1);
        
        PointInfos[] data = new PointInfos[count];
        _JellyReaderOutBuffer.GetData(data);

        return data;
    }
    
    #endregion

    [Button()]
    public void CaptureObstacleMask()
    {
        Vector2 spriteSize = _JellyRenderer.transform.localScale;
        
        _MapSize.x = (int)spriteSize.x;
        _MapSize.y = (int)spriteSize.y;
        
        _MapSize = _MapSize.MulByFloat(_TextureQuality);
        
        _ObstacleMaskPreview = _ObstacleMaskGenerator.GenerateMask(_JellyRenderer.transform.position,
            spriteSize, _MapSize.x, _MapSize.y, _ObstacleLayer);
        
        Vector2Int size = new Vector2Int((int) (_MapSize.x), (int) (_MapSize.y ));
        
        _JellyMaskInstance = new RenderTexture(size.x, size.y, 16, RenderTextureFormat.ARGB32);
        
        Init();
        //_ComputeShaderJellyMask.GetKernelThreadGroupSizes(_MainKernel, out _KernelGroupeSize.x,out _KernelGroupeSize.y, out _KernelGroupeSize.z);

        _ComputeShaderJellyMask.SetTexture(_RemoveJellyByMaskKernel, "_ObstaclesMask", _ObstacleMaskPreview);
        _ComputeShaderJellyMask.SetTexture(_RemoveJellyByMaskKernel, "_JellyMaskSource", _MapSource);
        _ComputeShaderJellyMask.SetTexture(_RemoveJellyByMaskKernel, "_JellyMask", _JellyMaskInstance);
        
        _ComputeShaderJellyMask.Dispatch(_RemoveJellyByMaskKernel, 
            (int) (_MapSize.x / _MainKernelGroupeSize.x), 
            (int) (_MapSize.y /_MainKernelGroupeSize.y),
            (int) _MainKernelGroupeSize.z);
        
        SaveMap(true);
    }


}

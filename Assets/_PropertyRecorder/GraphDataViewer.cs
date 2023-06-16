using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GraphDataViewer : MonoBehaviour
{
    public string _DataPath = "/_PropertyRecorder";
    public int _ReadIndex = 0;
    public GraphData _Data;
    private LineRenderer _LineRenderer;
    
    [Space] 
    [Range(0.00001f, 0.1f)] public float _WidthScale = 0.05f;
    [Range(0.1f, 3f)]public float _HeightsScale = 5f;
    
    [Button()]
    public void ReaderData()
    {
        _Data = PropertyRecorder.DecompressGraphDataJson(_DataPath);
        if (!_LineRenderer) _LineRenderer = GetComponent<LineRenderer>();

        _LineRenderer.positionCount = _Data._PropertiesData[_ReadIndex]._Keyframe.Count;
        for (int i = 0; i < _Data._PropertiesData[_ReadIndex]._Keyframe.Count; i++)
        {
            Vector2 pos = new Vector2(_Data._PropertiesData[_ReadIndex]._Keyframe[i] * _WidthScale, _Data._PropertiesData[_ReadIndex]._KeyframeValue[i] * _HeightsScale);
            _LineRenderer.SetPosition(i, pos.toVec3());
        }
    }

    private void OnValidate()
    {
        if (_Data == null || !_LineRenderer) return;
        
        for (int i = 0; i < _Data._PropertiesData[_ReadIndex]._Keyframe.Count; i++)
        {
            Vector2 pos = new Vector2(_Data._PropertiesData[_ReadIndex]._Keyframe[i] * _WidthScale, _Data._PropertiesData[_ReadIndex]._KeyframeValue[i] * _HeightsScale);
            _LineRenderer.SetPosition(i, pos.toVec3());
        }
    }
}

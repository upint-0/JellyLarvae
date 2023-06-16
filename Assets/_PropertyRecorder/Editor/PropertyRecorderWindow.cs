using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


public class PropertyRecorderWindow : EditorWindow
{
    public bool _Record;
    private bool _PropertyFinded;
    
    private string _FilePath = "/_PropertyRecorder";
    private string _FileName = "_GraphData";
    private float _TimeStep = 1f;
    private Object _Source;
    private string _ComponentName;
    private string _FieldName;
    
    private Vector2 _RecordPanelScrollPos;
    private Vector2 _ViewPanelScrollPos;
    
    [MenuItem("Window/Tool/PropertyRecorder")]
    public static void ShowWindow()
    {
        PropertyRecorderWindow wnd = GetWindow<PropertyRecorderWindow>();
        wnd.titleContent = new GUIContent("PropertyRecorderTool");
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        DrawRecordPanel();
        DrawViewerPanel();
        GUILayout.EndHorizontal();
        
    }

    private const float _RecordPanelWidth = 400f;
    private void DrawRecordPanel()
    {
        GUILayout.BeginVertical();
        _RecordPanelScrollPos = GUILayout.BeginScrollView(_RecordPanelScrollPos, GUILayout.Width(_RecordPanelWidth));
        
        GUILayout.Label("Object to observe : ");
        _Source = EditorGUILayout.ObjectField(_Source, typeof(Object), true);
        GUILayout.Label("Component Name : ");
        _ComponentName = GUILayout.TextField(_ComponentName);
        GUILayout.Label("Variable Name : ");
        _FieldName = GUILayout.TextField(_FieldName);

        if (GUILayout.Button("Select Variable"))
        {
            _PropertyFinded = SelectVariable();
        }

        if (_PropertyFinded)
        {
            GUILayout.Label("Property is valid");
        }
        else
        {
            GUILayout.Label("Property Not Finded");
        }
        
        GUILayout.Space(10f);
        GUILayout.Label("Time step : ");
        _TimeStep= EditorGUILayout.FloatField(_TimeStep);
        GUILayout.Space(10f);
        GUILayout.Label("File path : ");
        _FilePath = GUILayout.TextField(_FilePath);
        GUILayout.Label("File path : ");
        _FileName = GUILayout.TextField(_FileName);
        
        GUILayout.Space(10f);
        if (!_Record)
        {
            if (GUILayout.Button("Start Record"))
            {
                _Record = true;
                StartRecord();
            }
        }
        else
        {
            if (GUILayout.Button("Stop Record"))
            {
                _Record = false;
                StopCoroutine();
            }
        }
        GUILayout.Space(100f);
        _StrechToWindow = GUILayout.Toggle(_StrechToWindow, "Stretch to window");
        _StepValue = EditorGUILayout.IntSlider(_StepValue, 1, 200);
        _GraphObjectLoaded = EditorGUILayout.ObjectField(_GraphObjectLoaded, typeof(Object), false);
        if (GUILayout.Button("Load data"))
        {
            LoadData();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    // GRAPH
    private Object _GraphObjectLoaded;
    private GraphData _GraphLoaded;
    
    private float[] _DataValues;

    private float _GraphMinValue = 0f;
    private float _GraphMaxValue = 0f;
    private float _GraphHeight = 400f;
    private float _GraphWidth = 400f;
    private bool _StrechToWindow = true;
    private int _StepValue = 80;
    private float _RecordMaxTime;
    
    private const float _CaseMinWidth = 10f;
    private bool _IsLoaded = false;
    private void LoadData()
    {
        Debug.Log("Load data");
        _GraphLoaded = PropertyRecorder.DecompressGraphDataJson(_GraphObjectLoaded);
        _DataValues = _GraphLoaded._KeyframeValue.ToArray();
        _GraphMaxValue = 0f;
        for (int i = 0; i < _GraphLoaded._KeyframeValue.Count; i++)
        {
            if (_GraphMaxValue < _GraphLoaded._KeyframeValue[i]) _GraphMaxValue = _GraphLoaded._KeyframeValue[i];
        }

        _RecordMaxTime = _GraphLoaded._Keyframe[^1];
        _IsLoaded = true;
    }
    private void DrawViewerPanel()
    {
        GUILayout.BeginVertical();
        _ViewPanelScrollPos = EditorGUILayout.BeginScrollView(_ViewPanelScrollPos, GUILayout.Height(_GraphHeight + 20f));

        if (_IsLoaded && _GraphLoaded != null)
        {
            float caseWidth = Mathf.Max(_GraphWidth / _DataValues.Length, _CaseMinWidth);
            float graphWidth = caseWidth * (_DataValues.Length / (float)_StepValue);

            int step = _StepValue;
            if (_StrechToWindow)
            {
                graphWidth = position.width - _RecordPanelWidth - 20f;
                step = Mathf.RoundToInt(_DataValues.Length / (graphWidth/ caseWidth));
            }
            Rect graphRect = GUILayoutUtility.GetRect(graphWidth, position.height - 40f);
            //GUI.Box(new Rect(10f,10f, _GraphWidth, _GraphHeight), "");
            _GraphWidth = graphRect.width;
            _GraphHeight = graphRect.height;
            GUI.Box(graphRect, "");

            
            for (int i = 0; i < _DataValues.Length; i += step)
            {
                int index = i / step;
                float normalizedValue = Mathf.InverseLerp(_GraphMinValue, _GraphMaxValue, _DataValues[i]);
                //float xPos = (i / (float)(dataValues.Length - 1)) * (_GraphWidth - 20f);
                float xPos = index * caseWidth;
                float yPos = 10f + _GraphHeight - (normalizedValue * (_GraphHeight - 20f));
                
                GUI.color = new Color(0.5f,0.94f,0.675f);
                GUI.Box(new Rect(xPos, yPos, caseWidth, (_GraphHeight -20f) * normalizedValue), "", GraphStyle.BoxStyle(new Color(0.5f,0.94f,0.675f)));
            }

            float xMousePos = Event.current.mousePosition.x;
            float normaliedXPos = Mathf.Clamp01((xMousePos - position.x) / graphRect.width);
            int readIndex = Mathf.RoundToInt(normaliedXPos * (_DataValues.Length - 1));
            string debugText = "t : " + (normaliedXPos * _RecordMaxTime / 360f) + "s Value : " + _DataValues[readIndex];
            GUI.color = Color.white;
            GUI.Box(new Rect(graphRect.position.x, graphRect.position.y, graphRect.width, 36f), debugText, GraphStyle.ToolbarStyle(new Color(0.15f,0.15f,0.15f)));
            GUI.Box(new Rect(xMousePos, 36f, 0.2f , _GraphHeight - 36f), "", GraphStyle.BoxStyle(Color.red));
        }
        else
        {
            GUILayout.Label("Load data ...");
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
    
    
    private GraphData _GraphData;
    private EditorCoroutine _RecordCoroutine;
    private PropertyInfo _PropertyInfo;
    private Component _ComponentAttached;

    private bool SelectVariable()
    {
        if (_Source == null) return false;
        try
        {
            GameObject obj = _Source as GameObject;
            if (obj != null)
            {
                Component c = obj.GetComponent(_ComponentName);
                Type type = c.GetType();

                string exactPropertyName = "";
                PropertyInfo[] allProperties = type.GetProperties();

                foreach (var p in allProperties)
                {
                    if (p.Name.Contains(_FieldName))
                    {
                        exactPropertyName = p.Name;
                    }
                }
                PropertyInfo propertyInfo = type.GetProperty(exactPropertyName);
                
                if (propertyInfo != null)
                {
                    _PropertyInfo = propertyInfo;
                    _ComponentAttached = c;
                    Debug.Log("Property finded");
                    return true;
                }
                else
                {
                    Debug.Log("Failed to get property to the type");
                    return false;
                }
            }
            else
            {
                Debug.Log("Failed to get object");
                return false;
            }
        }
        catch(Exception e)
        {
            Debug.LogError("Can find the variable" + e);
            return false;
        }
    }
    private void StartRecord()
    {
        _GraphData = new GraphData();
        
        _RecordCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(
            RecordCoroutine(_GraphData, _TimeStep));
    }

    private void StopCoroutine()
    {
        if (_RecordCoroutine != null)
        {
            EditorCoroutineUtility.StopCoroutine(_RecordCoroutine);
            PropertyRecorder.SaveGraphToJson(_GraphData, _FilePath,_FileName + "_"+ _PropertyInfo.Name);
        }
    }

    private void OnDisable()
    { 
        StopCoroutine();
    }

    private IEnumerator RecordCoroutine(GraphData data,float timeStep)
    {
        while (_Record && _PropertyFinded)
        {
            yield return new WaitForSeconds(timeStep);
            Debug.Log("SaveData");
            if (data._Keyframe.Count > 0)
            {
                float previousKeytime = data._Keyframe[^1];
                data._Keyframe.Add(previousKeytime + timeStep);
            }
            else
            {
                data._Keyframe.Add(timeStep);
            }

            object value = _PropertyInfo.GetValue(_ComponentAttached);
            float fValue = 5f;
            
            if (value is float)
            {
                fValue = (float)value;
            } else if (value is int)
            {
                fValue = (float) ((int)value);
            }
            data._KeyframeValue.Add(fValue);
        }
    }
}
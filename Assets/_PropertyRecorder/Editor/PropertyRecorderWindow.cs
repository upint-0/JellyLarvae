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
    
    [MenuItem("Window/Tool/PropertyRecorder")]
    public static void ShowWindow()
    {
        PropertyRecorderWindow wnd = GetWindow<PropertyRecorderWindow>();
        wnd.titleContent = new GUIContent("PropertyRecorderTool");
    }

    private void OnGUI()
    {
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
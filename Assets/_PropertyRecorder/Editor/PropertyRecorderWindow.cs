using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Unity.EditorCoroutines.Editor;
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

    private bool _AddPropertyToRecordPanel;
    private Vector2 _RecordPanelScrollPos;
    private Vector2 _ViewPanelScrollPos;

    #region Style 
    private GUISkin _GUISkin;
    private Texture2D _RecordIcon;
    #endregion

    private PropertySelector _PropertySelector;
    private bool _IsInit = false;
    
    [MenuItem("Window/Analysis/Property Recorder")]
    public static void ShowWindow()
    {
        PropertyRecorderWindow wnd = GetWindow<PropertyRecorderWindow>();
        wnd.titleContent = new GUIContent("PropertyRecorderTool");
    }

    private void CreateGUI()
    {
        Init();
    }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        _GUISkin = Resources.Load("Style/PropertyRecorderStyle") as GUISkin;
        _PropertySelector = new PropertySelector();
        _IsInit = true;
    }

    private void OnDestroy()
    {
        _IsInit = false;
    }

    private void OnGUI()
    {
        if(!_IsInit) Init();
        GUILayout.BeginHorizontal();
        DrawRecordPanel();
        DrawViewerPanel();
        GUILayout.EndHorizontal();
        
    }

    private void ResetValue()
    {
        _ComponentAttached = new List<Component>();
        _PropertyInfo = new List<PropertyInfo>();
    }

    private const float _RecordPanelWidth = 400f;

    public struct PropertySelector
    {
        public Object _Source;
        public Component[] _Components;
        public string[] _ComponentsName;
        public int _ComponentIndex;
        public PropertyInfo[] _PropertyAvailable;
        [CanBeNull] public string[] _PropertyAvailableName;
        public int _PropertyInfoIndex;
    }
    private void DrawRecordPanel()
    {
        GUILayout.BeginVertical();
        _RecordPanelScrollPos = GUILayout.BeginScrollView(_RecordPanelScrollPos, GUILayout.Width(_RecordPanelWidth));

        if (_ComponentAttached.Count() > 0)
        {
            if (GUILayout.Button("Reset"))
            {
                ResetValue();
            }
        }
        if (_PropertyInfo.Count > 0)
        {
            foreach (var info in _PropertyInfo)
            {
                GUILayout.Label("property : " +info.Name);
            }
        }
        if (!_AddPropertyToRecordPanel)
        {
            if (GUILayout.Button("Add property to record"))
            {
                _AddPropertyToRecordPanel = true;
            }
        }
        
        if (_AddPropertyToRecordPanel)
        {
            EditorGUI.BeginChangeCheck();
            _Source = EditorGUILayout.ObjectField("GameObject to observe",_Source, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
            {
                GetAllComponent();
            }

            if (_PropertySelector._Components is { Length: > 0 })
            {
                EditorGUI.BeginChangeCheck();
                _PropertySelector._ComponentIndex =
                    EditorGUILayout.Popup("Components", _PropertySelector._ComponentIndex, _PropertySelector._ComponentsName);
                if (EditorGUI.EndChangeCheck())
                {
                    GetAllProperties();
                    Debug.Log(_PropertySelector._Components[_PropertySelector._ComponentIndex]);
                }

                if (_PropertySelector._PropertyAvailable != null)
                {
                    if (_PropertySelector._PropertyAvailable.Length > 0)
                    {
                        EditorGUI.BeginChangeCheck();
                        _PropertySelector._PropertyInfoIndex =
                            EditorGUILayout.Popup("Property", _PropertySelector._PropertyInfoIndex,
                                _PropertySelector._PropertyAvailableName);
                        if (EditorGUI.EndChangeCheck())
                        {
                            // Update property 
                            Debug.Log(_PropertySelector._PropertyAvailable[_PropertySelector._PropertyInfoIndex]);
                        }
                    }
                    else
                    {
                        GUILayout.Label("No properties available ... ");
                    }
                }

            }

            if (GUILayout.Button("Select Variable"))
            {
                _PropertyFinded = SelectVariable();
            }
        }


        GUILayout.Space(10f);
        _TimeStep = GraphStyle.CreateFloatField(ref _TimeStep, "Time step");
        GUILayout.Space(10f);
        _FilePath = GraphStyle.CreateTextField(ref _FilePath, "File path");
        _FileName = GraphStyle.CreateTextField(ref _FileName, "File name");
        
        GUILayout.Space(10f);
        if (!_Record)
        {
            if (GUILayout.Button("Start Record", GraphStyle.StartRecordButtonStyle(_GUISkin)))
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
                StopRecord();
            }
        }
        GUILayout.Space(100f);
        _StrechToWindow = GUILayout.Toggle(_StrechToWindow, "Stretch to window");
        _StepValue = EditorGUILayout.IntSlider(_StepValue, 1, 600);
        _GraphObjectLoaded = EditorGUILayout.ObjectField(_GraphObjectLoaded, typeof(Object), false);
        if (GUILayout.Button("Load data"))
        {
            LoadData();
        }

        GUILayout.Space(10f);
        if (_IsLoaded)
        {
            for (int i = 0; i < _GraphLoaded._PropertiesData.Count(); i++)
            {
                if(_ViewPropertyGraph.Count() <= i) _ViewPropertyGraph.Add(true);
                
                Color color = GraphStyle.GetColorByIndex(i);
                GraphStyle.GuiLine(color, 10);
                _ViewPropertyGraph[i] = GUILayout.Toggle(_ViewPropertyGraph[i],
                    _GraphLoaded._PropertiesData[i]._PropertyName + " values");
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    // GRAPH
    private Object _GraphObjectLoaded;
    private GraphData _GraphLoaded;
    
    //private float[] _DataValues;

    private float _GraphMinValue = 0f;
    private float[] _GraphMaxValue;
    private float _GraphHeight = 400f;
    private float _GraphWidth = 400f;
    private bool _StrechToWindow = true;
    private int _StepValue = 80;
    private float _RecordMaxTime;
    
    private const float _CaseMinWidth = 10f;
    private bool _IsLoaded = false;

    private List<bool> _ViewPropertyGraph = new List<bool>();
    private void LoadData()
    {
        Debug.Log("Load data");
        _GraphLoaded = PropertyRecorder.DecompressGraphDataJson(_GraphObjectLoaded);
        _GraphMaxValue = new float[_GraphLoaded._PropertiesData.Count];
        for (int y = 0; y < _GraphMaxValue.Count(); y++)
        {
            for (int i = 0; i < _GraphLoaded._PropertiesData[y]._KeyframeValue.Count; i++)
            {
                if (_GraphMaxValue[y] < _GraphLoaded._PropertiesData[y]._KeyframeValue[i]) _GraphMaxValue[y] = _GraphLoaded._PropertiesData[y]._KeyframeValue[i];
            }
        }

        _RecordMaxTime = _GraphLoaded._PropertiesData[0]._Keyframe[^1];
        _IsLoaded = true;
    }
    private void DrawViewerPanel()
    {
        GUILayout.BeginVertical();
        _ViewPanelScrollPos = EditorGUILayout.BeginScrollView(_ViewPanelScrollPos, GUILayout.Height(_GraphHeight + 20f));

        if (_IsLoaded && _GraphLoaded != null)
        {
            int dataCount = _GraphLoaded._PropertiesData[0]._Keyframe.Count;
            float caseWidth = Mathf.Max(_GraphWidth / dataCount, _CaseMinWidth);
            float graphWidth = caseWidth * (dataCount / (float)_StepValue);

            int step = _StepValue;
            if (_StrechToWindow)
            {
                graphWidth = position.width - _RecordPanelWidth - 20f;
                step = Mathf.RoundToInt(dataCount / (graphWidth/ caseWidth));
            }
            Rect graphRect = GUILayoutUtility.GetRect(graphWidth, position.height - 40f);
            //GUI.Box(new Rect(10f,10f, _GraphWidth, _GraphHeight), "");
            _GraphWidth = graphRect.width;
            _GraphHeight = graphRect.height;
            GUI.Box(graphRect, "");

            float xMousePos = Event.current.mousePosition.x;
            float normaliedXPos = Mathf.Clamp01((xMousePos - position.x) / graphRect.width);
            int readIndex = Mathf.RoundToInt(normaliedXPos * (dataCount - 1));
            string debugText = "t : " + (normaliedXPos * _RecordMaxTime / 360f) + "s ";

            float maxValue = 0f;
            for (int j = 0; j < _GraphLoaded._PropertiesData.Count(); j++)
            {
                if (!_ViewPropertyGraph[j]) continue;
                if (_GraphMaxValue[j] >= maxValue) maxValue = _GraphMaxValue[j];
            }

            for (int j = 0; j < _GraphLoaded._PropertiesData.Count(); j++)
            {
                if (!_ViewPropertyGraph[j]) continue;
                PropertyData data = _GraphLoaded._PropertiesData[j];
                
                for (int i = 0; i < dataCount; i += step)
                {
                    int index = i / step;
                    float normalizedValue = Mathf.InverseLerp(_GraphMinValue, maxValue, data._KeyframeValue[i]);
                    //float xPos = (i / (float)(dataValues.Length - 1)) * (_GraphWidth - 20f);
                    float xPos = index * caseWidth;
                    float yPos = 10f + _GraphHeight - (normalizedValue * (_GraphHeight - 20f));

                    Color color = GraphStyle.GetColorByIndex(j);
                    GUI.color = color;
                    GUI.Box(new Rect(xPos, yPos, caseWidth, (_GraphHeight -20f) * normalizedValue), "", GraphStyle.BoxStyle(color));
                }

                debugText += _GraphLoaded._PropertiesData[j]._PropertyName + " : " + _GraphLoaded._PropertiesData[j]._KeyframeValue[readIndex] + " ";
            }
            
            

           
            GUI.color = Color.white;
            GUI.Box(new Rect(graphRect.position.x, graphRect.position.y, graphRect.width, 36f), debugText, GraphStyle.ToolbarStyle(new Color(0.15f,0.15f,0.15f)));
            //GUI.Box(new Rect(xMousePos, 36f, 0.02f , _GraphHeight - 36f), "", GraphStyle.BoxStyle(Color.red));
            EditorGUI.DrawRect(new Rect(xMousePos, 36f, 2f , _GraphHeight), Color.red);
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
    private List<PropertyInfo> _PropertyInfo = new List<PropertyInfo>();
    private List<Component> _ComponentAttached = new List<Component>();

    private void GetAllComponent()
    {
        if (_Source == null) return;

        GameObject obj = _Source as GameObject;
        if (obj != null)
        {
            _PropertySelector._Components = obj.GetComponents(typeof(Component));
            _PropertySelector._ComponentsName = _PropertySelector._Components.Select(component => component.GetType().ToString()).ToArray();
        }

        GetAllProperties();
    }

    private void GetAllProperties()
    {
        if(_PropertySelector._Components is not { Length: > 0 }) return;

        Component c = _PropertySelector._Components[_PropertySelector._ComponentIndex];
        Type type = c.GetType();
        _PropertySelector._PropertyAvailable = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.DeclaringType == c.GetType())
            .ToArray();
        _PropertySelector._PropertyAvailableName = _PropertySelector._PropertyAvailable.Select(property => property.ToString()).ToArray();
    }

    private bool SelectVariable()
    {
        if (_Source == null) return false;
        if (_PropertySelector._PropertyAvailable == null) return false;
        if (_PropertySelector._PropertyAvailable.Length <= 0) return false;
        
        _PropertyInfo.Add(_PropertySelector._PropertyAvailable[_PropertySelector._PropertyInfoIndex]);
        _ComponentAttached.Add(_PropertySelector._Components[_PropertySelector._ComponentIndex]);
        _PropertySelector = new PropertySelector();
        _Source = null;
        
        _AddPropertyToRecordPanel = false;
        return true;
    }
    
    #region Record Methods
    private void StartRecord()
    {
        _GraphData = new GraphData();
        for (int i = 0; i < _PropertyInfo.Count(); i++)
        {
            _GraphData._PropertiesData.Add(new PropertyData());
        }
        _RecordCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(
            RecordCoroutine(_GraphData, _TimeStep));
    }
    private void StopRecord()
    {
        if (_RecordCoroutine != null)
        {
            EditorCoroutineUtility.StopCoroutine(_RecordCoroutine);
            string fileName = _FileName;

            for (int i = 0; i < _PropertyInfo.Count(); i++)
            {
                fileName += "_" + _PropertyInfo[i].Name;
            }
            PropertyRecorder.SaveGraphToJson(_GraphData, _FilePath,fileName);
        }
    }
    private IEnumerator RecordCoroutine(GraphData data,float timeStep)
    {
        while (_Record && _PropertyFinded)
        {
            yield return new WaitForSeconds(timeStep);
            

            for (int i = 0; i < data._PropertiesData.Count; i++)
            {
                
                if (data._PropertiesData[i]._Keyframe.Count > 0)
                {
                    float previousKeytime = data._PropertiesData[i]._Keyframe[^1];
                    data._PropertiesData[i]._Keyframe.Add(previousKeytime + timeStep);
                }
                else
                {
                    data._PropertiesData[i]._Keyframe.Add(timeStep);
                }
                
                
                object value = _PropertyInfo[i].GetValue(_ComponentAttached[i]);
                
                float fValue = 5f;
            
                if (value is float)
                {
                    fValue = (float)value;
                } else if (value is int)
                {
                    fValue = Convert.ToSingle((int)value);
                }
                else
                {
                    Debug.LogError("Unsupported type " + value.GetType());
                }

                data._PropertiesData[i]._PropertyName = _PropertyInfo[i].Name;
                data._PropertiesData[i]._KeyframeValue.Add(fValue);
            }

        }
    }
    #endregion
    private void OnDisable()
    { 
        StopRecord();
    }


}
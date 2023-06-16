using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PropertyRecorder : MonoBehaviour
{
    private GraphData _GraphData;
    
    public static void SaveGraphToJson(GraphData data,string filePath, string fileName)
    {
        string graph = JsonUtility.ToJson(data);
        string path = Application.dataPath + filePath;
        Debug.Log(path);
        System.IO.FileInfo file = new System.IO.FileInfo(path);
        if (file.Directory != null) file.Directory.Create();
        System.IO.File.WriteAllText(path + "/" + fileName + ".json", graph);
    }

    public static GraphData DecompressGraphDataJson(string filePath)
    {
        string absolutePath = Application.dataPath + filePath;
        string serializedData = System.IO.File.ReadAllText(absolutePath);
        GraphData data = JsonUtility.FromJson<GraphData>(serializedData);
        return data;
    }
    public static GraphData DecompressGraphDataJson(Object obj)
    {
        string filePath = AssetDatabase.GetAssetPath(obj);
        //string absolutePath = Application.persistentDataPath + filePath;
        string serializedData = System.IO.File.ReadAllText(filePath);
        GraphData data = JsonUtility.FromJson<GraphData>(serializedData);
        return data;
    }
}

[System.Serializable]
public class GraphData
{
    public List<PropertyData> _PropertiesData = new List<PropertyData>();
}

[System.Serializable]
public class PropertyData
{
    public string _PropertyName;
    public List<float> _Keyframe= new List<float>();
    public List<float> _KeyframeValue = new List<float>();
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphStyle : MonoBehaviour
{
    public static GUIStyle BoxStyle(Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.normal.background = MakeTex(2, 2, color);
        
        return style;
    }

    public static GUIStyle ToolbarStyle(Color color)
    {
        GUIStyle style = BoxStyle(color);
        style.alignment = TextAnchor.MiddleLeft;
        style.padding = new RectOffset(8,8,8,8);
        style.fontSize = 20;
        return style;
    }

    private static Color[] colors = new Color[]
    {
        new Color(0.48f,0.77f,0.59f, 0.85f),
        new Color(0.48f,0.71f,0.77f, 0.85f),
        new Color(0.91f,0.43f,0.4f, 0.85f),
        new Color(0.91f,0.769f,0.435f, 0.85f),
        new Color(0.435f,0.91f,0.882f, 0.85f),
        new Color(0.439f,0.435f,0.91f, 0.85f),
        new Color(0.91f,0.435f,0.879f, 0.85f),
        new Color(0.879f,0.91f,0.435f, 0.85f),
    };
    public static Color GetColorByIndex(int i)
    {
        return colors[i % colors.Length];
    }
    
    public static void GuiLine(Color color, int i_height = 1 )
    {
        Rect rect = EditorGUILayout.GetControlRect(false, i_height );

        rect.height = i_height;

        EditorGUI.DrawRect(rect, color);
    }
    
    private static Texture2D MakeTex( int width, int height, Color col )
    {
        Color[] pix = new Color[width * height];
        for( int i = 0; i < pix.Length; ++i )
        {
            pix[ i ] = col;
        }
        Texture2D result = new Texture2D( width, height );
        result.SetPixels( pix );
        result.Apply();
        return result;
    }

    public static float CreateFloatField(ref float output, string label = "")
    {
        return EditorGUILayout.FloatField(label, output);
    }
    public static string CreateTextField(ref string output, string label = "")
    {
        return EditorGUILayout.TextField(label, output);
    }

    public static GUIStyle FieldStyle()
    {
       return new GUIStyle()
       {
           alignment = TextAnchor.MiddleLeft,
           padding = new RectOffset(8,8,8,8)
       }; 
    }

    public static GUIStyle StartRecordButtonStyle(GUISkin skin)
    {
        return skin.GetStyle("StartRecordButton");
    }

}

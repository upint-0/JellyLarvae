using System.Collections;
using System.Collections.Generic;
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

}

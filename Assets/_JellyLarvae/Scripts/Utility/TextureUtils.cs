using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureUtils
{
    public enum FileFormat
    {
        JPG, PNG, TGA
    };

    public enum Channel
    {
        R,
        RG,
        RGB,
        RGBA
    }

    public static void Export(this Texture2D tex, string path, string fileName = "default", FileFormat format = FileFormat.PNG, int jpgQuality = 100)
    {
        switch (format)
        {
            case FileFormat.JPG:
                System.IO.File.WriteAllBytes(path + fileName + ".jpg", tex.EncodeToJPG(jpgQuality));
                break;
            case FileFormat.PNG:
                System.IO.File.WriteAllBytes(path + fileName + ".png", tex.EncodeToPNG());
                break;
            case FileFormat.TGA:
                System.IO.File.WriteAllBytes(path + fileName + ".tga", tex.EncodeToTGA());
                break;
        }
    }
    public static void ExportToTexture2D(this RenderTexture rt, string path, string fileName = "default", Channel channel = Channel.RGBA, FileFormat fileFormat = FileFormat.PNG, int jpgQuality = 100)
    {
        TextureFormat format = TextureFormat.RGBA32;;
        switch (channel)
        {
            case Channel.R:
                format = TextureFormat.R8;
                break;
            case Channel.RG:
                format = TextureFormat.RG16;
                break;
            case Channel.RGB:
                format = TextureFormat.RGB24;
                break;
            case Channel.RGBA:
                format = TextureFormat.RGBA32;
                break;
        }
        Texture2D tex = new Texture2D(rt.width, rt.height, format, false, false);
        
        var previousRT = RenderTexture.active;
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0,0, rt.width, rt.height), 0,0);
        tex.Apply();
        RenderTexture.active = previousRT;
        
        tex.Export(path, fileName, fileFormat, jpgQuality);

        if (Application.isPlaying)
        {
            Object.Destroy(tex);
        }
        else
        {
            Object.DestroyImmediate(tex);
        }
    }
}

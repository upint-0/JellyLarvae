using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ObstacleMaskGenerator : MonoBehaviour
{
    private void OnEnable()
    {
        gameObject.SetActive(false);
    }

    public RenderTexture GenerateMask(Vector2 center, Vector2 sizeWS, int width, int height, LayerMask layerToCapture)
    {
        Camera cam = GetComponent<Camera>();
        UniversalAdditionalCameraData camData = GetComponent<UniversalAdditionalCameraData>();
        cam.cullingMask = layerToCapture;
        cam.orthographic = true;
        cam.transform.position = center.toVec3(-10f);
        float minSize = Mathf.Min(sizeWS.x, sizeWS.y) / 2f;
        cam.orthographicSize = minSize;
        camData.renderShadows = false;
        cam.backgroundColor = Color.black;
        
        RenderTexture maskRT = new RenderTexture(width, height, 24);
        cam.targetTexture = maskRT;
        cam.Render();
        
        return maskRT;
    }
}

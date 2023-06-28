using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovementNetwork : NetworkBehaviour
{
    [SerializeField] private float _Speed = 4f;
    private Camera _Camera;
    private bool _IsInit = false;
    private Vector3 _MouseInput;

    private void Init()
    {
        _Camera = Camera.main;
    }

    private void Update()
    {
        if (!IsOwner || !Application.isFocused) return;
        _MouseInput.x = Input.mousePosition.x;
        _MouseInput.y = Input.mousePosition.y;
        _MouseInput.z = _Camera.nearClipPlane;

        Vector3 mouseWS = _Camera.ScreenToWorldPoint(_MouseInput);
        transform.position = Vector3.MoveTowards(transform.position, mouseWS, Time.deltaTime * _Speed);

        if (mouseWS != transform.position)
        {
            Vector3 targetDir = mouseWS - transform.position;
            targetDir.z = 0f;
            transform.up = targetDir;
        }
    }

    #region Network

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Init();
        Debug.Log("Spawn player");
    }

    #endregion
    
    
    
    /**
     * Dynamically Spawned : Awake - OnNetworkSpawn - Start
     * In-Scene Placed : Awake - Start - OnNetworkSpawn
     */
}
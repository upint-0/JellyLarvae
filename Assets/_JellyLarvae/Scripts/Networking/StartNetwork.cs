using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartNetwork : MonoBehaviour
{
    public GameObject _NetworkPanel;
    public GameObject _ShowNetworkButton;
    private bool _IsShowed;
    private void Start()
    {
        _NetworkPanel.SetActive(true);
        _ShowNetworkButton.SetActive(false);
        _IsShowed = true;
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        _NetworkPanel.SetActive(false);
        _ShowNetworkButton.SetActive(true);
        _IsShowed = false;
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        _NetworkPanel.SetActive(false);
        _ShowNetworkButton.SetActive(true);
        _IsShowed = false;
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        _NetworkPanel.SetActive(false);
        _ShowNetworkButton.SetActive(true);
        _IsShowed = false;
    }

    public void ShowNetworkPanel()
    {
        _IsShowed = !_IsShowed;
        _NetworkPanel.SetActive(_IsShowed);
        _ShowNetworkButton.SetActive(true);
    }
}

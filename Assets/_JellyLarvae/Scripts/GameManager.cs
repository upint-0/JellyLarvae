using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager _Instance;
    public PlayerEntity _Player;
    
    [Header(("Input"))] 
    [SerializeField] private KeyCode _RestartKeycode = KeyCode.R;
    [SerializeField] private KeyCode _PauseKeycode = KeyCode.P;
    private bool _IsPaused;

    private void Awake()
    {
        if (_Instance)
        {
            Destroy(this);
        }
        else
        {
            _Instance = this;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(_RestartKeycode))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(_PauseKeycode))
        {
            Time.timeScale = (_IsPaused) ? 1f: 0f;
            _IsPaused = !_IsPaused;
        }
    }
}

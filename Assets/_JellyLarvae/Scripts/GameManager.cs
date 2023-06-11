using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum E_GameState
    {
        InGame,
        Paused,
        GameOver,
    }

    [SerializeField] private E_GameState _CurrentGameState = E_GameState.InGame;
    public E_GameState CurrentGameState => _CurrentGameState;
    
    public static GameManager _Instance;
    public PlayerEntity _Player;
    
    [Header(("Input"))] 
    [SerializeField] private KeyCode _RestartKeycode = KeyCode.R;
    [SerializeField] private KeyCode _PauseKeycode = KeyCode.P;

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
        switch (_CurrentGameState)  
        {
            case E_GameState.InGame:
                if (Input.GetKeyDown(_RestartKeycode))
                {
                    RestartGame();
                }
                if (Input.GetKeyDown(_PauseKeycode))
                {
                    SwitchGameState(E_GameState.Paused);
                }
                break;
            case E_GameState.Paused:
                if (Input.GetKeyDown(_PauseKeycode))
                {
                    SwitchGameState(E_GameState.InGame);
                }
                break;
            case E_GameState.GameOver:
                if (Input.GetKeyDown(_RestartKeycode))
                {
                    RestartGame();
                }
                break;
        }
    }

    public delegate void OnChangeGameState();
    public void SwitchGameState(E_GameState newState)
    {
        ExitPreviousGameState();
        _CurrentGameState = newState;
        EnterInNewGameState();
    }

    private void EnterInNewGameState()
    {
        switch (_CurrentGameState)  
        {
            case E_GameState.InGame:
                break;
            case E_GameState.Paused:
                Time.timeScale = 0f;
                break;
            case E_GameState.GameOver:
                GameOver();
                break;
        }
    }

    private void ExitPreviousGameState()
    {
        switch (_CurrentGameState)  
        {
            case E_GameState.InGame:
                break;
            case E_GameState.Paused:
                Time.timeScale = 1f;
                break;
            case E_GameState.GameOver:
                break;
        }
    }
    private void GameOver()
    {
        Time.timeScale = 0f;
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

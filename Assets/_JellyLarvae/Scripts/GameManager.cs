using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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

    [FormerlySerializedAs("_TimeSpeed")]
    [Header("Debug")] 
    [SerializeField] private float _TimeScale = 1f;
    [Header(("Input"))] 
    [SerializeField] private KeyCode _RestartKeycode = KeyCode.R;
    [SerializeField] private KeyCode _PauseKeycode = KeyCode.P;

    private int _BestScore;
    public int BestScore => _BestScore;
    private bool _IsInit = false;

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

    private void Start()
    {
        _BestScore = DataManager._Instance.GetBestScore();
        _IsInit = true;
    }

    private void OnEnable()
    {
        PlayerEntity.onLevelChanged += OnPlayerLevelChanged;
    }

    private void OnDisable()
    {
        PlayerEntity.onLevelChanged -= OnPlayerLevelChanged;
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

    [Button()]
    public void ApplyTimeScale()
    {
        Time.timeScale = _TimeScale;
    }

    public delegate void OnChangeGameState(E_GameState state);
    public static event OnChangeGameState onChangeGameState;
    
    public void SwitchGameState(E_GameState newState)
    {
        ExitPreviousGameState();
        _CurrentGameState = newState;
        EnterInNewGameState();
        // Call event 
        onChangeGameState?.Invoke(newState);
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

    private void OnPlayerLevelChanged(int level)
    {
        if (level > _BestScore && _IsInit)
        {
            DataManager._Instance.SaveBestScore(level);
            _BestScore = level;
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

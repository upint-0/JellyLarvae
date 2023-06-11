using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GameManager;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _TextPlayerLevel;
    [Space] 
    [SerializeField] private GameObject _GameOverPanel;
    [SerializeField] private TextMeshProUGUI _BestscoreGameOverPanel;

    private void OnEnable()
    {
        GameManager.onChangeGameState += GameStateIsUpdated;
        PlayerEntity.onLevelChanged += PlayerLevelIsUpdated;
        _GameOverPanel.SetActive(false);
    }

    private void OnDisable()
    {
        GameManager.onChangeGameState -= GameStateIsUpdated;
        PlayerEntity.onLevelChanged -= PlayerLevelIsUpdated;
    }
    
    private void PlayerLevelIsUpdated(int level)
    {
        _TextPlayerLevel.text = "Level : " + level;
    }
    private void GameStateIsUpdated(E_GameState state)
    {
        switch (state)
        {
            case E_GameState.GameOver:
                _GameOverPanel.SetActive(true);
                _BestscoreGameOverPanel.text = "Score : " + GameManager._Instance._Player.CurrentLevel + 
                                               "\n Best score : " + GameManager._Instance.BestScore;
                break;
            case E_GameState.Paused:
                break;
        }

    }
}

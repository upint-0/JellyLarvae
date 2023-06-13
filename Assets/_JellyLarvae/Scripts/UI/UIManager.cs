using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;
using static PropertyBonus;
public class UIManager : MonoBehaviour
{
    [Serializable]
    struct BonusIcon
    {
        public E_BonusType _BonusType;
        public Image _Img;
    }

    public static UIManager _Instance;
    
    [SerializeField] private TextMeshProUGUI _TextPlayerLevel;
    [Header("HUD")] 
    [SerializeField] private BonusIcon[] _BonusIcon;
    [Header("Game Over Panel")] 
    [SerializeField] private GameObject _GameOverPanel;
    [SerializeField] private TextMeshProUGUI _BestscoreGameOverPanel;

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
        foreach (var b in _BonusIcon)
        {
            b._Img.gameObject.SetActive(false);
        }
    }

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

    public void ActiveDisableBonus(bool active, int bonusType)
    {
        Debug.Log("Active bonus : " + bonusType);
        foreach (var b in _BonusIcon)
        {
            if ((int)b._BonusType == bonusType)
            {
                b._Img.gameObject.SetActive(active);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class PlayerEntity : MonoBehaviour
{
    [Expandable, SerializeField] private PlayerAttributesSO _PlayerAttributes;
    [SerializeField] private int _CurrentLevel;
    public int CurrentLevel => _CurrentLevel;

    private void Awake()
    {
        _CurrentLevel = _PlayerAttributes.BaseLevel;

    }

    private void Start()
    {
        RefreshLevel();
    }

    private void Upgrade()
    {
        CameraFollowBehaviour._Instance.ImpulseZoom();
    }
    private void Downgrade()
    {
        CameraFollowBehaviour._Instance.PlayCameraShake();
        if (_CurrentLevel <= 0)
        {
            Death();
        }
    }
    private void Death()
    {
        GameManager._Instance.SwitchGameState(GameManager.E_GameState.GameOver);
    }
    private void RefreshLevel()
    {
        onLevelChanged?.Invoke(_CurrentLevel);
    }

    public delegate void OnLevelChanged(int level);
    public static event OnLevelChanged onLevelChanged;
    
    public bool InteractWithEnemy(int enemyLevel, int enemyPoint)
    {
        if (enemyLevel >= _CurrentLevel)
        {
            _CurrentLevel -= enemyPoint;
            Downgrade();
            RefreshLevel();
            return false;
        }
        else
        {
            _CurrentLevel += enemyPoint;
            Upgrade();
            RefreshLevel();
            return true;
        }  
    }

    #region Bonus
    public void CollectPoint(int point)
    {
        _CurrentLevel += point;
        RefreshLevel();
    }
    #endregion

}

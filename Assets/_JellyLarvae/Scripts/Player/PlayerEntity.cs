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
    [Space]
    [SerializeField] private TextMeshProUGUI _LevelText;
    private void Awake()
    {
        _CurrentLevel = _PlayerAttributes.BaseLevel;
        RefreshLevel();
    }

    private void Upgrade()
    {
        
    }
    private void Downgrade()
    {
        if (_CurrentLevel <= 0)
        {
            Death();
        }
    }
    private void Death()
    {
        Debug.Log("The player is dead !!");
    }
    private void RefreshLevel()
    {
        _LevelText.text = "Level : " + _CurrentLevel;
    }
    
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
}

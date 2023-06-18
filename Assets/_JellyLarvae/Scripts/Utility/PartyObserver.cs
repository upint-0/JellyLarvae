#define PARTY_OBSERVER_MODE

using System.Collections.Generic;
using UnityEngine;

public class PartyObserver : MonoBehaviour
{
    public static PartyObserver _Instance;

    public int _MaxEnemyLevel;
    public int _MaxEnemyDamage;
    public int _AverageEnemiesLevel;
    
    public int MaxEnemyLevel => _MaxEnemyLevel;
    public int MaxEnemyDamage => _MaxEnemyDamage;
    public int AverageEnemiesLevel => _AverageEnemiesLevel;

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

    public void RefreshEnemies()
    {
        List<BaseEnemy> enemies = EnemyManager._Instance.EnemiesList;

        int enemyLevel = 0;
        int enemyMaxLevel = 0;
        int enemyMaxDamage = 0;
        
        for (int i = 0; i < enemies.Count; i++)
        {
            enemyLevel += enemies[i]._Level;
            if (enemies[i]._Level >= enemyMaxLevel) enemyMaxLevel = enemies[i]._Level;
            if (enemies[i]._CurrentDamage >= enemyMaxDamage) enemyMaxDamage = enemies[i]._CurrentDamage;
        }

        _AverageEnemiesLevel = enemyLevel / enemies.Count;
        _MaxEnemyLevel = enemyMaxLevel;
        _MaxEnemyDamage = enemyMaxDamage;
    }
}

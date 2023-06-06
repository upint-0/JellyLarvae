using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager _Instance;
    [SerializeField] private int _EnemyCounter;
    public int EnemyCounter => _EnemyCounter;

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

    public void AddEnemy()
    {
        _EnemyCounter++;
    }

    public void RemoveEnemy()
    {
        _EnemyCounter--;
    }
}

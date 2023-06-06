using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpawnerHelper;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager _Instance;
    [Header("Waves")] 
    [SerializeField] private bool _ContinusSpawning = true;
    [SerializeField] private float _TimeBetweenWave = 30f;

    [SerializeField] private SpawnAttributes[] _EnemiesBaseWave;
    [SerializeField] [Range(0,1)] private float _ProgressionPrecentStrenght = 0.5f;
    private float _CurrentPercentProgression = 1f;
    private SpawnAttributes[] _EnemiesCurrentWave;

    [Space] 
    [SerializeField] private int _MaxEnemiesAlive = 128;
    [Header("Debug")]
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

        _EnemiesCurrentWave = _EnemiesBaseWave;
    }

    private void Start()
    {
        StartCoroutine(WaveCoroutine());
    }

    IEnumerator WaveCoroutine()
    {
        while (_ContinusSpawning)
        {
            yield return new WaitForSeconds(_TimeBetweenWave);
            int predictCount = _EnemyCounter;
            
            _CurrentPercentProgression += _ProgressionPrecentStrenght;
            
            for (int i = 0; i < _EnemiesBaseWave.Length; i++)
            {
                predictCount +=  Mathf.CeilToInt(_EnemiesBaseWave[i]._Number * _CurrentPercentProgression);
            }
            Debug.Log("Predict count " + predictCount);
            
            if (_MaxEnemiesAlive > predictCount)
            {
                int playerLevel = GameManager._Instance._Player.CurrentLevel;


                for (int i = 0; i < _EnemiesCurrentWave.Length; i++)
                {
                    _EnemiesCurrentWave[i]._Number =
                        Mathf.CeilToInt(_EnemiesBaseWave[i]._Number * _CurrentPercentProgression);
                    _EnemiesCurrentWave[i]._EnemyLevel =
                        playerLevel + _EnemiesCurrentWave[i]._EnemyBase.EnemyAttr.Level;
                }

                SpawnerHelper._Instance.Spawn(_EnemiesCurrentWave, transform);
            }
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

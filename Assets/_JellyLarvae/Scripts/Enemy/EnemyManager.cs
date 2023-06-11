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
    
    private int[] _EnemyCounterByType;

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
        _EnemyCounterByType = new int[_EnemiesBaseWave.Length];
    }

    private void Start()
    {
        StartCoroutine(WaveCoroutine());
    }

    IEnumerator WaveCoroutine()
    {
        while (_ContinusSpawning)
        {

            _CurrentPercentProgression += _ProgressionPrecentStrenght;
            

                int playerLevel = GameManager._Instance._Player.CurrentLevel;


                for (int i = 0; i < _EnemiesCurrentWave.Length; i++)
                {
                    int numberOfEnemyToSpawn = Mathf.CeilToInt(_EnemiesBaseWave[i]._Number * _CurrentPercentProgression);
                    numberOfEnemyToSpawn = Mathf.Min(_EnemiesBaseWave[i]._MaxNumberAlive - _EnemyCounterByType[i], numberOfEnemyToSpawn);
                    
                    _EnemiesCurrentWave[i]._Number = numberOfEnemyToSpawn;
                    _EnemiesCurrentWave[i]._EnemyLevel = playerLevel;
                    _EnemiesCurrentWave[i]._TypeID = i;

                    _EnemyCounterByType[i] += numberOfEnemyToSpawn;
                }

                SpawnerHelper._Instance.Spawn(_EnemiesCurrentWave, transform);
            
            yield return new WaitForSeconds(_TimeBetweenWave);
        }
    }

    public void AddEnemy()
    {
        _EnemyCounter++;
    }

    public void RemoveEnemy(int typeID)
    {
        _EnemyCounter--;
        _EnemyCounterByType[typeID]--;
    }
}

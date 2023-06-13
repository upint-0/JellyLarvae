using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static SpawnerHelper;
using Random = UnityEngine.Random;

public class CollectableManager : MonoBehaviour
{
    #region Variables
    public static CollectableManager _Instance;
    [Header("Waves")] 
    [SerializeField] private bool _ContinusSpawning = true;
    [SerializeField] private float _MinTimeBetweenSpawning = 10f;
    [SerializeField] private float _MaxTimeBetweenSpawning = 20f;

    [SerializeField] private SpawnableAttributes[] _CollectablesToSpawn;
    [SerializeField] [Range(0,1)] private float _ProgressionPrecentStrenght = 0.5f;
    private float _CurrentPercentProgression = 1f;
    private SpawnableAttributes[] _CollectableCurrentWave;

    private int[] _CollectableCounterByType;
    #endregion
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

        _CollectableCurrentWave = _CollectablesToSpawn;
        _CollectableCounterByType = new int[_CollectablesToSpawn.Length];
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
                
                for (int i = 0; i < _CollectableCurrentWave.Length; i++)
                {
                    int numberToSpawn = Mathf.CeilToInt(_CollectablesToSpawn[i]._Number * _CurrentPercentProgression);
                    numberToSpawn = Mathf.Min(_CollectablesToSpawn[i]._MaxNumberAlive - _CollectableCounterByType[i], numberToSpawn);
                    
                    _CollectableCurrentWave[i]._Number = numberToSpawn;
                    _CollectableCurrentWave[i]._TypeID = i;

                    _CollectableCounterByType[i] += numberToSpawn;
                }

                SpawnerHelper._Instance.Spawn(_CollectableCurrentWave, transform, true);

                float time = Random.Range(_MinTimeBetweenSpawning, _MaxTimeBetweenSpawning);
                yield return new WaitForSeconds(time);
        }
    }
    

    public void RemoveCollectable(int typeID)
    {
        _CollectableCounterByType[typeID]--;
    }
}

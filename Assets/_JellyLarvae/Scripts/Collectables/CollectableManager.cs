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

    [Header("Special")] 
    [SerializeField, Min(0)] private float _LevelCheckingTick = 5f;
    [SerializeField] private SpecialBonus[] _SpecialBonus; 
    
    private int[] _CollectableCounterByType;
    #endregion

    [Serializable]
    public struct SpecialBonus
    {
        public Collider2D _Collider;
        [Range(0, 1)] public float _LevelDifferencePercent;
        public int _MinLevelDifference;
        public int _MaxNumber;
        [HideInInspector] public int _CurrentNumber;

        public SpawnableAttributes ToSpawnableAttr(int id)
        {
            SpawnableAttributes attr = new SpawnableAttributes()
            {
                _Collider = _Collider,
                _Number = _CurrentNumber,
                _TypeID = id
            };


            return attr;
        }
    }
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

        _CollectableCurrentWave= new SpawnableAttributes[_CollectablesToSpawn.Length];
        for (int i = 0; i < _CollectablesToSpawn.Length; i++)
        {
            _CollectableCurrentWave[i] = _CollectablesToSpawn[i].GetCopy();
        }
        
        _CollectableCounterByType = new int[_CollectablesToSpawn.Length + _SpecialBonus.Length];
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Start()
    {
        StartCoroutine(WaveCoroutine());
        StartCoroutine(LevelCheckingForBonusCoroutine());
    }

    IEnumerator LevelCheckingForBonusCoroutine()
    {
        while (_ContinusSpawning)
        {
            yield return new WaitForSeconds(_LevelCheckingTick);

            for (int i = 0; i < _SpecialBonus.Length; i++)
            {
                int typeID = _CollectableCurrentWave.Length + i;
                if (_CollectableCounterByType[typeID] < _SpecialBonus[i]._MaxNumber)
                {
                    int playerLevel = GameManager._Instance.GetPlayerLevel();
                    int enemyLevel = EnemyManager._Instance.EnemyLevelAverage;
                    float ratio = playerLevel / (float)enemyLevel;
                    if (ratio <= _SpecialBonus[i]._LevelDifferencePercent &&
                        _SpecialBonus[i]._MinLevelDifference <= enemyLevel - playerLevel)
                    {
                        _CollectableCounterByType[typeID]++;
                        // Spawn special bonus 
                        
                        SpawnerHelper._Instance.Spawn(_SpecialBonus[i].ToSpawnableAttr(typeID), transform, true);
                        Debug.Log("Spawn special bonus !");
                    }
                }
            }


        }
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

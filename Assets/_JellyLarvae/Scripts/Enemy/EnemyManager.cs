using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpawnerHelper;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    #region Variables
    public static EnemyManager _Instance;
    [Header("Waves")] 
    [SerializeField] private bool _ContinusSpawning = true;
    [SerializeField] private float _TimeBetweenWave = 30f;

    [SerializeField] private SpawnableAttributes[] _EnemiesBaseWave;
    [Space]
    [SerializeField] [Range(0,1)] private float _NumberProgressionPrecent = 0.5f;
    [SerializeField] [Range(0,1)] private float _LevelProgressionPrecent = 0.5f;
    
    private float _CurrentNumberPercentProgression = 1f;
    private float _CurrentLevelPercentProgression = 0f;
    private SpawnableAttributes[] _EnemiesCurrentWave;
    
    [Header("Debug")]
    [SerializeField] private int _EnemyCounter;
    public int EnemyCounter => _EnemyCounter;
    
    private int[] _EnemyCounterByType;
    
    #if PARTY_OBSERVER_MODE
    private List<BaseEnemy> _EnemiesList= new List<BaseEnemy>();
    public List<BaseEnemy> EnemiesList => _EnemiesList;
    #endif
    
    
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
        
        _EnemiesCurrentWave= new SpawnableAttributes[_EnemiesBaseWave.Length];
        for (int i = 0; i < _EnemiesBaseWave.Length; i++)
        {
            _EnemiesCurrentWave[i] = _EnemiesBaseWave[i].GetCopy();
        }
        
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

            _CurrentNumberPercentProgression += _NumberProgressionPrecent;
            

                int playerLevel = GameManager._Instance._Player.CurrentLevel;


                for (int i = 0; i < _EnemiesCurrentWave.Length; i++)
                {
                    int numberOfEnemyToSpawn = Mathf.CeilToInt(_EnemiesBaseWave[i]._Number * _CurrentNumberPercentProgression);
                    numberOfEnemyToSpawn = Mathf.Min(_EnemiesBaseWave[i]._MaxNumberAlive - _EnemyCounterByType[i], numberOfEnemyToSpawn);
                    
                    _EnemiesCurrentWave[i]._Number = numberOfEnemyToSpawn;

                    int levelComp = (int) (Random.Range(
                        _EnemiesBaseWave[i]._EnemyBase.EnemyAttr.MinLevel,
                        _EnemiesBaseWave[i]._EnemyBase.EnemyAttr.MaxLevel) * _LevelProgressionPrecent);
                    
                    _EnemiesCurrentWave[i]._EnemyLevel = playerLevel + levelComp;
                    _EnemiesCurrentWave[i]._TypeID = i;

                    _EnemyCounterByType[i] += numberOfEnemyToSpawn;
                }

                SpawnerHelper._Instance.Spawn(_EnemiesCurrentWave, transform, false);
            
            yield return new WaitForSeconds(_TimeBetweenWave);
        }
    }

    public void AddEnemy(BaseEnemy enemyRef)
    {
        _EnemyCounter++;
        
        #if PARTY_OBSERVER_MODE
        _EnemiesList.Add(enemyRef);
        if(PartyObserver._Instance) PartyObserver._Instance.RefreshEnemies();
        #endif
    }

    public void RemoveEnemy(int typeID, BaseEnemy enemyRef)
    {
        _EnemyCounter--;
        _EnemyCounterByType[typeID]--;
        
        #if PARTY_OBSERVER_MODE
        _EnemiesList.Remove(enemyRef);
        if(PartyObserver._Instance) PartyObserver._Instance.RefreshEnemies();
        #endif
    }
}

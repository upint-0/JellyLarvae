using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SpawnerHelper : MonoBehaviour
{
    public static SpawnerHelper _Instance;
    
    [SerializeField] private SpawnableAttributes[] _ObjToSpawn;
    [SerializeField] private SpriteRenderer _Canvas;
    [SerializeField, Range(0f,10f)] private float _SafeRadiusToPlayer = 4f;
    private GameManager _GameManager;
    private bool _IsInit;
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

    [Button()]
    public void SpawnObj()
    {
        Spawn(_ObjToSpawn, transform, false);
    }

    private void Init()
    {
        _GameManager = GameManager._Instance;
        _IsInit = true;
    }

    private void OnDisable()
    {
        _IsInit = false;
    }

    [Serializable]
    public struct SpawnableAttributes
    {
        public BaseEnemy _EnemyBase;
        [HideInInspector] public int _EnemyLevel;
        [HideInInspector] public int _TypeID;
        public Collider2D _Collider;
        public int _Number;
        public int _MinSpaceToSpawn;
        public int _MaxNumberAlive;
        [Space] 
        public int _LevelStepToIncreaseDamage;


        
        public SpawnableAttributes GetCopy()
        {
            return new SpawnableAttributes()
            {
                _EnemyBase = this._EnemyBase,
                _EnemyLevel = this._EnemyLevel,
                _TypeID =  this._TypeID,
                _Collider = this._Collider,
                _Number = this._Number,
                _MaxNumberAlive = this._MaxNumberAlive,
                _LevelStepToIncreaseDamage = this._LevelStepToIncreaseDamage
            };
        }
        public SpawnableAttributes[] GetListCopy(SpawnableAttributes[] list)
        {
            SpawnableAttributes[] l = new SpawnableAttributes[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                l[i] = list[i].GetCopy();
            }
            return l;
        }
    }

    public void SpawnInCanvas(SpawnableAttributes[] attr,Vector2 center, Vector2 size, Transform parent, bool isCollectable)
    {
        int playerLevel = GameManager._Instance._Player.CurrentLevel;
        for (int i = 0; i < attr.Length; i++)
        {
            for (int j = 0; j < attr[i]._Number; j++)
            {
                TrySpawnObj(attr[i], center, size, parent, isCollectable, playerLevel);
            }
        }
    }
    public void Spawn(SpawnableAttributes[] attr, Transform parent, bool isCollectable)
    {
        if(!_IsInit) Init();
        
        int playerLevel = GameManager._Instance._Player.CurrentLevel;
        for (int i = 0; i < attr.Length; i++)
        {
            for (int j = 0; j < attr[i]._Number; j++)
            {
                TrySpawnObj(attr[i], _Canvas.transform.position, _Canvas.transform.localScale, parent, isCollectable, playerLevel);
            }
        }
    }
    
    private void TrySpawnObj(SpawnableAttributes obj, Vector2 center, Vector2 size, Transform parent, bool isCollectable, int playerLevel)
    {
        
        bool isSpawned = false;
        
        const int maxIteration = 50;
        int iteration = 0;
        
        float xMin, yMin, xMax, yMax;
        
        xMin = center.x - (size.x / 2f);
        yMin = center.y - (size.y / 2f);
        xMax = center.x + (size.x / 2f);
        yMax = center.y + (size.y / 2f);

        Collider2D col = obj._Collider;

        while (!isSpawned && iteration <= maxIteration)
        {
            Vector2 rdmPos = new Vector2
            (
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );
            float radius = Mathf.Max(Mathf.Max(col.bounds.size.x, col.bounds.size.y), col.bounds.size.z);;
            
            bool spaceOccupied = Physics2D.OverlapCircle(rdmPos, radius) || 
                                 (Vector2.Distance(rdmPos, _GameManager._Player.transform.position) < _SafeRadiusToPlayer);
            
            if (!spaceOccupied)
            {

                Quaternion randomRot = new Quaternion();
                if (obj._EnemyBase)
                {
                    // Spawn a enemy
                    BaseEnemy enemy = Instantiate<BaseEnemy>(obj._EnemyBase, rdmPos, randomRot.Random2DRotation(), parent);
                    enemy.Init(obj._EnemyLevel + Random.Range(enemy.EnemyAttr.MinLevel, 
                        enemy.EnemyAttr.MaxLevel), obj._TypeID, 
                        playerLevel / obj._LevelStepToIncreaseDamage);

                }
                else
                {
                    GameObject clone = Instantiate(col.gameObject, rdmPos, randomRot.Random2DRotation(), parent);
                    if (isCollectable)
                    {
                        clone.GetComponent<CollectableBase>()._TypeID = obj._TypeID;
                    }
                }

                isSpawned = true;
            }
            iteration++;
        }
    }
    
}

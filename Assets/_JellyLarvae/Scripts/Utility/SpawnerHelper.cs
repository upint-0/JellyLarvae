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
    
    [SerializeField] private SpawnAttributes[] _ObjToSpawn;
    [SerializeField] private SpriteRenderer _Canvas;

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
        Spawn(_ObjToSpawn, transform);
    }
    
    [Serializable]
    public struct SpawnAttributes
    {
        public BaseEnemy _EnemyBase;
        [HideInInspector] public int _EnemyLevel;
        public Collider2D _Collider;
        public int _Number;
    }
    public void SpawnInCanvas(SpawnAttributes[] attr,Vector2 center, Vector2 size, Transform parent)
    {
        for (int i = 0; i < attr.Length; i++)
        {
            for (int j = 0; j < attr[i]._Number; j++)
            {
                TrySpawnObj(attr[i], center, size, parent);
            }
        }
    }
    public void Spawn(SpawnAttributes[] attr, Transform parent)
    {
        for (int i = 0; i < attr.Length; i++)
        {
            for (int j = 0; j < attr[i]._Number; j++)
            {
                TrySpawnObj(attr[i], _Canvas.transform.position, _Canvas.transform.localScale, parent);
            }
        }
    }
    private void TrySpawnObj(SpawnAttributes obj, Vector2 center, Vector2 size, Transform parent)
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
            
            bool spaceOccuped = Physics2D.OverlapCircle(rdmPos, radius);
            if (!spaceOccuped)
            {

                Quaternion randomRot = new Quaternion();
                if (obj._EnemyBase)
                {
                    BaseEnemy enemy = Instantiate<BaseEnemy>(obj._EnemyBase, rdmPos, randomRot.Random2DRotation(), parent);
                    enemy._Level = obj._EnemyLevel;
                    
                    Debug.Log("Enemy spawned : " + obj._EnemyLevel);
                }
                else
                {
                    Instantiate(col.gameObject, rdmPos, randomRot.Random2DRotation(), parent);
                }

                isSpawned = true;
            }
            iteration++;
        }
    }
}

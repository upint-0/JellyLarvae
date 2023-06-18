using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField, Expandable] protected EnemyAttributesSO _enemyAttributes;
    public EnemyAttributesSO EnemyAttr => _enemyAttributes;
    
    public int _Level;
    [HideInInspector] public int _CurrentDamage;
    [HideInInspector] public int _TypeID;
    
    [Header("Level Renderer")] 
    [SerializeField] private SpriteRenderer _LevelSpriteRenderer;
    [SerializeField, ColorUsage(false, true)] private Color _LevelUpperColor = Color.red;
    [SerializeField, ColorUsage(false, true)] private Color _LevelLowerColor = Color.blue;
    protected PlayerEntity _PlayerRef;

    private void Awake()
    {
        if (_Level == 0)
        {
            _Level = Random.Range(_enemyAttributes.MinLevel, _enemyAttributes.MaxLevel);
            _CurrentDamage = _enemyAttributes.Damage;
        }
    }

    protected virtual void Start()
    {
        _PlayerRef = GameManager._Instance._Player;
        EnemyManager._Instance.AddEnemy();

    }
    

    protected virtual void Update()
    {
        if (!_LevelSpriteRenderer) return;
        _LevelSpriteRenderer.color = (PlayerLevelChecking()) ? _LevelUpperColor: _LevelLowerColor;
    }

    protected bool PlayerLevelChecking()
    {
        return (_PlayerRef.CurrentLevel <= _Level);
    }

    protected virtual void Death()
    {
        EnemyManager._Instance.RemoveEnemy(_TypeID);
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager._Instance._Player.InteractWithEnemy(_Level, _enemyAttributes.Point, _CurrentDamage);
            Death();
        }
    }
}

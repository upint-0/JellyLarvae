using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField, Expandable] protected EnemyAttributesSO _enemyAttributes;
    public EnemyAttributesSO EnemyAttr => _enemyAttributes;
    
    public int _Level;
    
    [Header("Level Renderer")] 
    [SerializeField] private SpriteRenderer _LevelSpriteRenderer;
    [SerializeField, ColorUsage(false, true)] private Color _LevelUpperColor = Color.red;
    [SerializeField, ColorUsage(false, true)] private Color _LevelLowerColor = Color.blue;
    protected PlayerEntity _PlayerRef;

    protected virtual void Start()
    {
        _PlayerRef = GameManager._Instance._Player;
        EnemyManager._Instance.AddEnemy();
        if(_Level == 0)_Level = _enemyAttributes.Level;
    }
    

    protected virtual void Update()
    {
        if (!_LevelSpriteRenderer) return;
        _LevelSpriteRenderer.color = (PlayerLevelChecking()) ? _LevelUpperColor: _LevelLowerColor;
    }

    private bool PlayerLevelChecking()
    {
        return (_PlayerRef.CurrentLevel <= _Level);
    }

    protected virtual void Death()
    {
        EnemyManager._Instance.RemoveEnemy();
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager._Instance._Player.InteractWithEnemy(_Level, _enemyAttributes.Point);
            Death();
        }
    }
}

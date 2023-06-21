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
    [SerializeField] private GameObject _flagella;
    private ParticleSystem.MainModule _flagellaMain;
    [SerializeField] private GameObject _ChangeLevelEffect;
    private ParticleSystem _ChangeLevelEffectPS;
    private ParticleSystem.MainModule _ChangeLevelEffectMain;
    [SerializeField, ColorUsage(false, true)] private Color _LevelUpperColor = Color.red;
    [SerializeField, ColorUsage(false, true)] private Color _LevelLowerColor = Color.blue;
    protected PlayerEntity _PlayerRef;

    private bool _IsMorePowerfull = true;

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
        EnemyManager._Instance.AddEnemy(this);

        if (_flagella != null)
        {
            _flagellaMain = _flagella.GetComponent<ParticleSystem>().main;
        }
        if (_ChangeLevelEffect != null)
        {
            _ChangeLevelEffectPS = _ChangeLevelEffect.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
            if (_ChangeLevelEffectPS != null) _ChangeLevelEffectMain = _ChangeLevelEffectPS.main;
        }
    }
    

    protected virtual void Update()
    {
        if (!_LevelSpriteRenderer) return;
        _LevelSpriteRenderer.color = (PlayerLevelChecking()) ? _LevelUpperColor: _LevelLowerColor;
        
        if (_flagella != null)
        {
            _flagellaMain.startColor = new ParticleSystem.MinMaxGradient(PlayerLevelChecking() ? _LevelUpperColor : _LevelLowerColor);
        }
    }

    protected bool PlayerLevelChecking()
    {
        bool isBetter = (_PlayerRef.CurrentLevel <= _Level);
        if(_IsMorePowerfull != isBetter)  ChangeLevelEffect();
        _IsMorePowerfull = isBetter;
        return isBetter;
    }

    protected virtual void ChangeLevelEffect()
    {
        if (_ChangeLevelEffect != null)
        {
            Debug.Log("Play particle system changing");
            
            _ChangeLevelEffectMain.startColor =
                new ParticleSystem.MinMaxGradient(_IsMorePowerfull ? _LevelUpperColor : _LevelLowerColor);
            
            _ChangeLevelEffectPS.Play();
        }
    }

    protected virtual void Death()
    {
        EnemyManager._Instance.RemoveEnemy(_TypeID, this);
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

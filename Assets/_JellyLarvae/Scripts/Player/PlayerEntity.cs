using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using static PropertyBonus;

public class PlayerEntity : MonoBehaviour
{
    [Serializable]
    public enum E_PlayerState
    {
        Alive,
        Invincible,
        Death
    }

    public ValueWrapper<E_PlayerState> _PlayerState;
    
    [Space]
    [Expandable, SerializeField] private PlayerAttributesSO _PlayerAttributes;
    [SerializeField] private int _CurrentLevel;
    [SerializeField] private PlayerEffect _PlayerEffect;
    public int CurrentLevel => _CurrentLevel;
    public float Test = 5f;
    public PlayerMovement _PlayerMvt;
    private Coroutine BonusCoroutine;
    private Dictionary<int, Coroutine> BonusCoroutineDict = new Dictionary<int, Coroutine>();
    
    private void Awake()
    {
        _CurrentLevel = _PlayerAttributes.BaseLevel;
    }

    private void Start()
    {
        RefreshLevel();
    }
    

    private void Upgrade()
    {
        CameraFollowBehaviour._Instance.ImpulseZoom();
        _PlayerEffect.Eat();
    }
    private void Downgrade()
    {
        CameraFollowBehaviour._Instance.PlayCameraShake();
        _PlayerEffect.Damaged();
        if (_CurrentLevel <= 0)
        {
            Death();
        }
    }
    private void Death()
    {
        GameManager._Instance.SwitchGameState(GameManager.E_GameState.GameOver);
    }
    private void RefreshLevel()
    {
        onLevelChanged?.Invoke(_CurrentLevel);
    }

    public delegate void OnLevelChanged(int level);
    public static event OnLevelChanged onLevelChanged;
    
    public bool InteractWithEnemy(int enemyLevel, int enemyPoint, int damage)
    {
        switch (_PlayerState.Value)
        {
            case E_PlayerState.Alive: 
                if (enemyLevel > _CurrentLevel)
                {
                    _CurrentLevel -= damage;
                    Downgrade();
                    RefreshLevel();
                    return false;
                }
                else
                {
                    _CurrentLevel += enemyPoint;
                    Upgrade();
                    RefreshLevel();
                    return true;
                }
            case E_PlayerState.Invincible :
                _CurrentLevel += enemyPoint;
                Upgrade();
                RefreshLevel();
                return true;
            default:
                return false;
        }

    }

    #region Bonus
    public void CollectPoint(int point)
    {
        _CurrentLevel += point;
        _PlayerEffect.Eat();
        RefreshLevel();
    }

    public void CollectPropertyBonus(ValueWrapper<float> property, float bonusValue, float time, E_BonusType type)
    {
        Coroutine c;
        int id = (int) type;
        if (BonusCoroutineDict.TryGetValue(id, out c))
        {
            StopCoroutine(c);
            c = StartCoroutine(SetTempoBonus(property, null, bonusValue, time, id));
            BonusCoroutineDict[id] = c;
        }
        else
        {
            c = StartCoroutine(SetTempoBonus(property, null, bonusValue, time, id));
            BonusCoroutineDict.Add(id, c);
            UIManager._Instance.ActiveDisableBonus(true,id);
        }
    }
    public void CollectPropertyBonus(ValueWrapper<E_PlayerState> property, float bonusValue, float time, E_BonusType type)
    {
        Coroutine c;
        int id = (int) type;
        if (BonusCoroutineDict.TryGetValue(id, out c))
        {
            StopCoroutine(c);
            c = StartCoroutine(SetTempoBonus(null, property, bonusValue, time, id));
            BonusCoroutineDict[id] = c;
        }
        else
        {
            c = StartCoroutine(SetTempoBonus(null, property, bonusValue, time, id));
            BonusCoroutineDict.Add(id, c);
            UIManager._Instance.ActiveDisableBonus(true,id);
        }
    }

    private IEnumerator SetTempoBonus(ValueWrapper<float> property, ValueWrapper<E_PlayerState> state, float bonusValue, float time, int id)
    {
        if (property != null)
        {
            property.Value = bonusValue;
        }
        else
        {
            state.Value = E_PlayerState.Invincible;
        }
        yield return new WaitForSeconds(time);
        if (property != null)
        {
            property.Value = property.BaseValue;
        }
        else
        {
            state.Value = state.BaseValue;
        }
        
        BonusCoroutineDict[id] = null;
        BonusCoroutineDict.Remove(id);
        UIManager._Instance.ActiveDisableBonus(false,id);
    }
    #endregion

}

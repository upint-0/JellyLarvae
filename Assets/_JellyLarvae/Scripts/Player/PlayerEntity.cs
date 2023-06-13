using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using static PropertyBonus;

public class PlayerEntity : MonoBehaviour
{
    [Expandable, SerializeField] private PlayerAttributesSO _PlayerAttributes;
    [SerializeField] private int _CurrentLevel;
    public int CurrentLevel => _CurrentLevel;
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
    }
    private void Downgrade()
    {
        CameraFollowBehaviour._Instance.PlayCameraShake();
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
    
    public bool InteractWithEnemy(int enemyLevel, int enemyPoint)
    {
        if (enemyLevel >= _CurrentLevel)
        {
            _CurrentLevel -= enemyPoint;
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
    }

    #region Bonus
    public void CollectPoint(int point)
    {
        _CurrentLevel += point;
        RefreshLevel();
    }

    public void CollectPropertyBonus(ValueWrapper<float> property, float bonusValue, float time, E_BonusType type)
    {
        Coroutine c;
        int id = (int) type;
        if (BonusCoroutineDict.TryGetValue(id, out c))
        {
            StopCoroutine(c);
            c = StartCoroutine(SetTempoBonus(property, bonusValue, time, id));
            BonusCoroutineDict[id] = c;
        }
        else
        {
            c = StartCoroutine(SetTempoBonus(property, bonusValue, time, id));
            BonusCoroutineDict.Add(id, c);
            UIManager._Instance.ActiveDisableBonus(true,id);
        }

    }

    private IEnumerator SetTempoBonus(ValueWrapper<float> property, float bonusValue, float time, int id)
    {
        property.Value = bonusValue;
        yield return new WaitForSeconds(time);
        property.Value = property.BaseValue;

        BonusCoroutineDict[id] = null;
        BonusCoroutineDict.Remove(id);
        UIManager._Instance.ActiveDisableBonus(false,id);
    }
    #endregion

}

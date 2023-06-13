using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyBonus : CollectableBase
{
    public enum  E_BonusType
    {
        Speed,
        Dash
    }
    

    [SerializeField] private E_BonusType _BonusType;
    [SerializeField] private float _BonusValue = 100f;
    [SerializeField] private float _BonusTime = 5f;
    protected override void Collect()
    {
        base.Collect();
        PlayerEntity player = GameManager._Instance._Player;
        switch (_BonusType)
        {
            case E_BonusType.Speed:
                player.CollectPropertyBonus(player._PlayerMvt._CurrentMaxSpeed, _BonusValue, _BonusTime, _BonusType);
                break;
            case E_BonusType.Dash:
                player.CollectPropertyBonus(player._PlayerMvt._CurrentDashForce, _BonusValue, _BonusTime, _BonusType);
                break;
        }
        
        
        Destroy(gameObject);
    }
}

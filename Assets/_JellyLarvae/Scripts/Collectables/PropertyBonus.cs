using UnityEngine;

public class PropertyBonus : CollectableBase
{
    #region Enums
    public enum  E_BonusType
    {
        Speed,
        Dash, 
        Invinsibility
    }
    #endregion
    #region Variables
    [SerializeField] private E_BonusType _BonusType;
    [SerializeField] private float _BonusValue = 100f;
    [SerializeField] private float _BonusTime = 5f;
    #endregion

    #region Control
    protected override void Collect()
    {
        PlayerEntity player = GameManager._Instance._Player;
        switch (_BonusType)
        {
            case E_BonusType.Speed:
                player.CollectPropertyBonus(player._PlayerMvt._CurrentMaxSpeed, _BonusValue, _BonusTime, _BonusType);
                break;
            case E_BonusType.Dash:
                player.CollectPropertyBonus(player._PlayerMvt._CurrentDashForce, _BonusValue, _BonusTime, _BonusType);
                break;
            case E_BonusType.Invinsibility:
                player.CollectPropertyBonus(player._PlayerState, _BonusValue, _BonusTime, _BonusType);
                break;
        }
        base.Collect();
        Destroy(gameObject);
    }
    #endregion

}

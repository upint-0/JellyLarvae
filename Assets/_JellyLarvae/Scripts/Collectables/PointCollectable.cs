using UnityEngine;

public class PointCollectable : CollectableBase
{
    [Header("Bonus - Point")] 
    [SerializeField] private int _Point;
    protected override void Collect()
    {
        GameManager._Instance._Player.CollectPoint(_Point);
        base.Collect();
        Destroy(gameObject);
    }
}

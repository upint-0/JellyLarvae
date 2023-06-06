using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCollectable : CollectableBase
{
    [Header("Bonus - Point")] 
    [SerializeField] private int _Point;
    protected override void Collect()
    {
        base.Collect();
        GameManager._Instance._Player.CollectPoint(_Point);
        Destroy(gameObject);
    }
}

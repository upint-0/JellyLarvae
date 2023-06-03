using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [SerializeField, Expandable] protected EnemyAttributesSO _enemyAttributes;


    protected virtual void Death()
    {
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager._Instance._Player.InteractWithEnemy(_enemyAttributes.Level, _enemyAttributes.Point);
            Death();
        }
    }
}

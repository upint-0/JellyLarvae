using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_MovableEnemyType
{
    Track,
    TrackAndAvoid,
    Avoid
}
[RequireComponent(typeof(Rigidbody2D))]
public class MovableEnemy : BaseEnemy
{
    [SerializeField] private E_MovableEnemyType _Type;
    
    private Rigidbody2D _Rigidbody;
    
    protected override void Start()
    {
        base.Start();
        _Rigidbody = GetComponent<Rigidbody2D>();

    }

    private void FixedUpdate()
    {
        switch (_Type)
        {
            case E_MovableEnemyType.Track:
                TrackPlayer();
                break;
            case E_MovableEnemyType.Avoid:
                AvoidPlayer();
                break;
            case E_MovableEnemyType.TrackAndAvoid:
                if (_PlayerRef.CurrentLevel > _enemyAttributes.Level)
                {
                    AvoidPlayer();
                }
                else
                {
                    TrackPlayer();
                }
                break;
        }
        TrackPlayer();
    }

    private void TrackPlayer()
    {
        Vector2 currentDir = Vector2.Lerp(transform.right, GetDirectionToPlayer(), _enemyAttributes.AngularSpeed * Time.fixedDeltaTime);

        Debug.DrawRay(transform.position,  (currentDir * 10f).toVec3(), Color.red);
        HandleMovement(currentDir);
        
    }

    private void AvoidPlayer()
    {
        Vector2 currentDir = Vector2.Lerp(transform.right, -GetDirectionToPlayer(), _enemyAttributes.AngularSpeed * Time.fixedDeltaTime);
        Debug.DrawRay(transform.position,  (currentDir * 10f).toVec3(), Color.red);
        HandleMovement(currentDir);
    }

    private Vector2 GetDirectionToPlayer()
    {
        Vector2 dirToPlayer = (_PlayerRef.transform.position - transform.position ).normalized;
        return dirToPlayer;
    }

    private void HandleMovement(Vector2 dir)
    {
        float z_rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);
        
        _Rigidbody.AddForce(transform.right * Time.fixedDeltaTime * _enemyAttributes.Speed, ForceMode2D.Force);
    }
}

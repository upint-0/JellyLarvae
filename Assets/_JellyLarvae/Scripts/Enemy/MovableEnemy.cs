using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovableEnemy : BaseEnemy
{
    private PlayerEntity _PlayerRef;
    private Rigidbody2D _Rigidbody;
    private void Start()
    {
        _PlayerRef = GameManager._Instance._Player;
        _Rigidbody = GetComponent<Rigidbody2D>();

    }

    private void FixedUpdate()
    {
        TrackPlayer();
    }

    private void TrackPlayer()
    {
        if (_PlayerRef)
        {
            Vector2 dirToPlayer = (_PlayerRef.transform.position - transform.position ).normalized;
            Vector2 currentDir = Vector2.Lerp(transform.right, dirToPlayer, _enemyAttributes.AngularSpeed * Time.fixedDeltaTime);

            Debug.DrawRay(transform.position,  (currentDir * 10f).toVec3(), Color.red);
            HandleMovement(currentDir);
        }
    }

    private void HandleMovement(Vector2 dir)
    {
        float z_rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);
        
        _Rigidbody.AddForce(transform.right * Time.fixedDeltaTime * _enemyAttributes.Speed, ForceMode2D.Force);
    }
}

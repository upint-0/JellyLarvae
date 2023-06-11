using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

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

    private bool _AvoidObstacle;
    private bool _DirToLeft;

    [SerializeField] private bool _IsAffectedByJelly = true;
    [SerializeField] private float _JellyGravity = 1;
    [SerializeField] private float _WorldGravity = 0.05f;
    
    protected override void Start()
    {
        base.Start();
        _Rigidbody = GetComponent<Rigidbody2D>();
        
        if(_IsAffectedByJelly)JellySurfaceDetection._Instance.AddJellyReader(
            GetInstanceID(), 
            transform.position, 
            transform.position, 
            true, 
            5f);
    }

    protected override void Death()
    {
        if(_IsAffectedByJelly) JellySurfaceDetection._Instance.RemoveJellyReader(GetInstanceID());
        base.Death();
    }
    
    protected override void Update()
    {
        JellySurfaceDetection._Instance.UpdateJellyReaderPosition(GetInstanceID(), transform.position, transform.position);
        base.Update();
    }
    private void FixedUpdate()
    {
        //_AvoidObstacle = DetectObstacles();
        //if (!_AvoidObstacle)
        if (_IsAffectedByJelly)
        {
            _Rigidbody.gravityScale = JellySurfaceDetection._Instance.GetJellyValue(GetInstanceID()) > 0.1f
                ? _JellyGravity
                : _WorldGravity;
        }
        

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
                    if (_PlayerRef.CurrentLevel > _enemyAttributes.MinLevel)
                    {
                        AvoidPlayer();
                    }
                    else
                    {
                        TrackPlayer();
                    }

                    break;
            }
        }
        /*else
        {
            Debug.Log("Avoid obstacle");
            Vector2 dir = (_DirToLeft) ? -transform.up : transform.up;
            Debug.DrawRay(transform.position,  (dir * 10f).toVec3(), Color.cyan);
            HandleMovement(dir);
            // Obstacle behaviour
        }*/
    }

    private void TrackPlayer()
    {
       HandleMovement(GetDirectionToPlayer());
    }

    private void AvoidPlayer()
    {
        HandleMovement(-GetDirectionToPlayer());
    }

    private bool DetectObstacles()
    {
        Debug.DrawRay(transform.position,  (transform.right * _enemyAttributes.RangeObstacleDetection), Color.red);
        if (Physics2D.Raycast(transform.position, 
                transform.right, 
                _enemyAttributes.RangeObstacleDetection,
                _enemyAttributes.ObstaclesLayer))
        {
            if (_AvoidObstacle == false)
            {
                Debug.Log("New dir for avoid");
                _DirToLeft = (Random.Range(0,2) == 0);
            }
            return true;
        }

        return false;
    }

    private Vector2 GetDirectionToPlayer()
    {
        Vector2 dirToPlayer = (_PlayerRef.transform.position - transform.position ).normalized;
        return dirToPlayer;
    }

    private void HandleMovement(Vector2 targetDir)
    {
        Vector2 dir = Vector2.Lerp(transform.right, targetDir, _enemyAttributes.AngularSpeed * Time.fixedDeltaTime);
        
        float z_rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);
        
        _Rigidbody.AddForce(transform.right * Time.fixedDeltaTime * _enemyAttributes.Speed, ForceMode2D.Force);
    }
    
}

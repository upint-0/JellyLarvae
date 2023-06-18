using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public enum E_MovableEnemyType
{
    Track,
    TrackAndAvoid,
    Avoid
}

public enum E_EnemyState
{
    Track,
    RandomWalk, 
    AvoidPlayer,
    Death
}
[RequireComponent(typeof(Rigidbody2D))]
public class MovableEnemy : BaseEnemy
{
    [SerializeField] private E_MovableEnemyType _Type;
    private E_EnemyState _CurrentState = E_EnemyState.Track;
    
    private Rigidbody2D _Rigidbody;

    private bool _AvoidObstacle;
    private bool _DirToLeft;
    private float _Speed;
    private float _DistToPlayer;
    private Vector2 _DirToPlayer;
    private Vector2 _CurrentDir;
    

    [SerializeField] private bool _IsAffectedByJelly = true;
    [SerializeField] private float _JellyGravity = 1;
    [SerializeField] private float _WorldGravity = 0.05f;
    
    protected override void Start()
    {
        base.Start();
        _Rigidbody = GetComponent<Rigidbody2D>();
        _Speed = Random.Range(_enemyAttributes.MinSpeed, _enemyAttributes.MaxSpeed);
        // Setup the jelly reader to the buffer of the compute shader 
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
        SwitchState(E_EnemyState.Death);
        base.Death();
    }
    
    protected override void Update()
    {
        JellySurfaceDetection._Instance.UpdateJellyReaderPosition(GetInstanceID(), transform.position, transform.position);
        base.Update();
    }
    private void FixedUpdate()
    {
        if (_CurrentState == E_EnemyState.Death) return;
        if (_IsAffectedByJelly)
        {
            _Rigidbody.gravityScale = JellySurfaceDetection._Instance.GetJellyValue(GetInstanceID()) > 0.1f
                ? _JellyGravity
                : _WorldGravity;
        }
        GetDirectionToPlayer();
        
        switch (_Type)
        {
            case E_MovableEnemyType.Track:
                TrackPlayer();
                break;
            case E_MovableEnemyType.Avoid:
                AvoidPlayer();
                break;
            case E_MovableEnemyType.TrackAndAvoid:
                // Level Difference 
                if (!PlayerLevelChecking())
                {
                    if(_CurrentState == E_EnemyState.Track) SwitchState(E_EnemyState.RandomWalk);
                }
                else
                {
                    SwitchState(E_EnemyState.Track);
                }
                
                switch (_CurrentState)
                {
                    case E_EnemyState.Track:
                        TrackPlayer();
                        break;
                    case E_EnemyState.AvoidPlayer :
                        AvoidPlayer();
                        if (_DistToPlayer > _enemyAttributes.RangePlayerDetection)
                        {
                            SwitchState(E_EnemyState.RandomWalk);
                        }
                        break;
                    case E_EnemyState.RandomWalk :
                        RandomWalk();
                        if (_DistToPlayer <= _enemyAttributes.RangePlayerDetection)
                        {
                            SwitchState(E_EnemyState.AvoidPlayer);
                        }
                        break;
                }
                break;
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

    public void SwitchState(E_EnemyState newState)
    {
        if (newState == _CurrentState) return;
        ExitPreviousState();
        _CurrentState = newState;
        EnterInNewState();
    }

    private void EnterInNewState()
    {
        switch (_CurrentState)
        {
            case E_EnemyState.Track:
                break;
            case E_EnemyState.AvoidPlayer :
                break;
            case E_EnemyState.RandomWalk :
                _CurrentDir = Random.insideUnitCircle.normalized;
                break;
            case E_EnemyState.Death : 
                break;
        }
    }

    private void ExitPreviousState()
    {
        switch (_CurrentState)
        {
            case E_EnemyState.Track:
                break;
            case E_EnemyState.AvoidPlayer :
                break;
            case E_EnemyState.RandomWalk :
                break;
            case E_EnemyState.Death : 
                break;
        }
    }

    private void TrackPlayer()
    {
       HandleMovement(_DirToPlayer);
    }

    private void AvoidPlayer()
    {
        HandleMovement(-_DirToPlayer);
    }

    private void RandomWalk()
    {
        if (DetectObstacles()) _CurrentDir =  Random.insideUnitCircle.normalized;
        HandleMovement(_CurrentDir);
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

    private void GetDirectionToPlayer()
    {
        Vector2 vecToPlayer = (_PlayerRef.transform.position - transform.position);
        _DistToPlayer = vecToPlayer.magnitude;
        _DirToPlayer =  vecToPlayer.normalized;
    }

    private void HandleMovement(Vector2 targetDir)
    {
        Vector2 dir = Vector2.Lerp(transform.right, targetDir, _enemyAttributes.AngularSpeed * Time.fixedDeltaTime);
        
        float z_rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);
        
        _Rigidbody.AddForce(transform.right * Time.fixedDeltaTime * _Speed, ForceMode2D.Force);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        var position = transform.position;
        Gizmos.DrawWireSphere(position, _enemyAttributes.RangePlayerDetection);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(position, position + (_DirToPlayer.toVec3() * _DistToPlayer));
    }
}

using UnityEngine;
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
    protected E_EnemyState _CurrentState = E_EnemyState.Track;
    
    protected Rigidbody2D _Rigidbody;

    private bool _AvoidObstacle;
    private bool _DirToLeft;
    private float _Speed;
    protected float _DistToPlayer;
    protected Vector2 _DirToPlayer;
    protected Vector2 _CurrentDir;
    

    [SerializeField] private bool _IsAffectedByJelly = true;
    [SerializeField] private float _JellyGravity = 0f;
    [SerializeField] private float _WorldGravity = 5f;
    [Header("Effect")]
    [SerializeField] private GameObject _TrailEffect;

    protected override void Start()
    {
        base.Start();
        Init();
    }

    protected virtual void Init()
    {
        _Rigidbody = GetComponent<Rigidbody2D>();
        _Speed = Random.Range(_enemyAttributes.MinSpeed, _enemyAttributes.MaxSpeed);
        // Setup the jelly reader to the buffer of the compute shader 
        if(_IsAffectedByJelly)JellySurfaceDetection._Instance.AddJellyReader(
            GetInstanceID(), 
            transform.position, 
            transform.position, 
            true, 
            5f);
        
        SwitchState(E_EnemyState.Track);
        _TrailEffect.SetActive(false);
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
    protected virtual void FixedUpdate()
    {
        if (_CurrentState == E_EnemyState.Death) return;
        if (_IsAffectedByJelly)
        {
            bool isInJelly = JellySurfaceDetection._Instance.GetJellyValue(GetInstanceID()) > 0.1f;
            _Rigidbody.gravityScale = isInJelly
                ? _JellyGravity
                : _WorldGravity;

            if (!isInJelly)
            {
                _Rigidbody.inertia = 1f;
            }
        }

        /*if (_Rigidbody.angularVelocity >= 1000f)
        {
            Debug.Log("Angular velocity up : " + _Rigidbody.angularVelocity + " name " + gameObject.name + " id " + GetInstanceID() + " pos " + transform.position);
        }*/
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
               TrackAndAvoidBehaviour();
                break;
        }
    }

    protected virtual void TrackAndAvoidBehaviour()
    {
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
                _TrailEffect.SetActive(false);
                break;
            case E_EnemyState.AvoidPlayer :
                _TrailEffect.SetActive(true);
                break;
            case E_EnemyState.RandomWalk :
                _TrailEffect.SetActive(true);
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

    protected virtual void TrackPlayer()
    {
       HandleMovement(_DirToPlayer);
    }

    protected virtual void AvoidPlayer()
    {
        HandleMovement(-_DirToPlayer);
    }

    protected virtual void RandomWalk()
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
                _DirToLeft = (Random.Range(0,2) == 0);
            }
            return true;
        }

        return false;
    }

    protected void GetDirectionToPlayer()
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

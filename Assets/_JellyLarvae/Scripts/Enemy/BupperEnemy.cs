using System.Collections;
using UnityEngine;

public class BupperEnemy : MovableEnemy
{
    [Header("Bupper")] 
    [SerializeField, Range(1f,50f)] private float _BumpStrenght = 20f;
    
    [SerializeField, Range(0.1f,10f)] private float _BumpMinTime = 1f;
    [SerializeField, Range(0.1f,10f)] private float _BumpMaxTime = 6f;
    private float _BumpTimeCooldown;

    protected override void Init()
    {
        base.Init();
        StartCoroutine(BumpCoroutine());
    }

    protected override void Death()
    {
        StopAllCoroutines();
        base.Death();
    }

    protected override void FixedUpdate()
    {
        Vector2 dir = Vector2.Lerp(transform.right, _CurrentDir, _enemyAttributes.AngularSpeed * Time.fixedDeltaTime);
        
        float z_rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);
        
        base.FixedUpdate();
    }


    // Override behaviour 
    protected override void TrackPlayer()
    {
        _CurrentDir = _DirToPlayer;
    }
    protected override void RandomWalk() { }

    protected override void AvoidPlayer()
    {
        _CurrentDir = -_DirToPlayer;
    }


    private IEnumerator BumpCoroutine()
    {
        while (_CurrentState != E_EnemyState.Death)
        {
            _BumpTimeCooldown = Random.Range(_BumpMinTime, _BumpMaxTime);
            yield return new WaitForSeconds(_BumpTimeCooldown);

            if (_CurrentState == E_EnemyState.RandomWalk)
            {
                _CurrentDir =  Random.insideUnitCircle.normalized;
            }
            BumpMovement();
        }
    }

    private void BumpMovement()
    {
        _Rigidbody.AddForce(transform.right *  _BumpStrenght, ForceMode2D.Impulse);
    }
}

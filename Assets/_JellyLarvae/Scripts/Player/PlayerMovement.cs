using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float Speed = 200.0f;
    [SerializeField] private float StopDistance = 1.0f;
    [SerializeField] private float DashForce = 40.0f;
    [SerializeField] private float DashCoolDown = 0.5f;

    [SerializeField] private JellyRenderer Jelly = null;

    [Header( "Debug" )]
    [SerializeField] private bool DebugMode = false;

    private Rigidbody2D _rigidbody2D = new Rigidbody2D();
    private SpriteRenderer _renderer = new SpriteRenderer();
    private Coroutine _dashRoutine = null;

    private Vector2 _direction = new Vector2();

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }
    private void Update()
    {
        HandleDash();

        if ( DetectJelly() )
        {
            EnterJelly();
        }
        else
        {
            ExitJelly();
        }
    }

    private void FixedUpdate()
    {
        _direction = ( Camera.main.ScreenToWorldPoint( Input.mousePosition ) - transform.position );
        float distance_to_mouse = _direction.magnitude;
        _direction = _direction.normalized;

        float z_rotation = Mathf.Atan2( _direction.y, _direction.x ) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler( 0.0f, 0.0f, z_rotation );

        HandleMovements( distance_to_mouse );
    }

    private void EnterJelly()
    {
        _rigidbody2D.gravityScale = 0.0f;

        if(DebugMode)
            _renderer.color = Color.green;
    }

    private void ExitJelly()
    {
        _rigidbody2D.gravityScale = 2.5f;

        if(DebugMode)
            _renderer.color = Color.red;
    }

    private bool DetectJelly()
    {
        //return Physics2D.OverlapCircle( transform.position, 1.0f, LayerMask.GetMask( "Wall" ) );
        return (Jelly.GetJellyValueAtPosition( transform.position, true, transform.position, 50f) > 0.1f);
    }

    private void HandleDash()
    {
        if ( Input.GetMouseButtonDown( 1 ) &&  _dashRoutine == null)
        {
            _dashRoutine = StartCoroutine( Dash() );
        }
    }

    private IEnumerator Dash()
    {
        _rigidbody2D.AddForce( _direction * DashForce, ForceMode2D.Impulse );
        yield return new WaitForSeconds( DashCoolDown );
        _dashRoutine = null;
    }

    private void HandleMovements( float distance_to_mouse )
    {
        if ( Input.GetMouseButton( 0 ) && distance_to_mouse > StopDistance )
        {
            _rigidbody2D.AddForce( _direction * Speed * Time.fixedDeltaTime * distance_to_mouse, ForceMode2D.Force );
        }
    }
}

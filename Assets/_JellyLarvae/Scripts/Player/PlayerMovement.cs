using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float Speed = 1.0f;
    [SerializeField] private float DashForce = 10.0f;
    [SerializeField] private float StopDistance = 1.0f;

    private Rigidbody2D _rigidbody2D = new Rigidbody2D();
    Vector2 _direction = new Vector2();

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        _direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        float distance_to_mouse = _direction.magnitude;
        _direction = _direction.normalized;

        float z_rotation = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);

        if (Input.GetMouseButton( 0 ) && distance_to_mouse > StopDistance)
        {
            _rigidbody2D.AddForce(_direction * Speed * Time.fixedDeltaTime * distance_to_mouse, ForceMode2D.Force);
        }
        if ( Input.GetMouseButtonDown( 1 ) && distance_to_mouse > StopDistance )
        { 
            _rigidbody2D.AddForce(_direction * DashForce, ForceMode2D.Impulse);
        }

        if (Physics2D.OverlapCircle(transform.position, 1.0f, LayerMask.GetMask("Wall")))
        {
            _rigidbody2D.gravityScale = 0.0f;
        }
        else
        {
            _rigidbody2D.gravityScale = 2.5f;
        }
    }

    /*private void OnCollisionEnter2D( Collision2D collision )
    {
        if ( ( ( 1 << collision.gameObject.layer ) & LayerMask.GetMask( "Wall" ) ) != 0 )
        {
            _rigidbody2D.gravityScale = 0.0f;
        }
    }

    private void OnCollisionExit2D( Collision2D collision )
    {
        if ( ( ( 1 << collision.gameObject.layer ) & LayerMask.GetMask( "Wall" ) ) != 0 )
        {
            _rigidbody2D.gravityScale = 10.0f;
        }
    }*/
}

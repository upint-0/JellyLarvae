using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float Speed = 1.0f;
    [SerializeField] private float StopDistance = 1.0f;

    private Rigidbody2D _rigidbody2D;
    Vector2 _direction = new Vector2();

    private void Start()
    {
        
    }

    private void Update()
    {
        _direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        float distance_to_mouse = _direction.magnitude;
        _direction = _direction.normalized;

        float z_rotation = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);
    
        if (Input.GetMouseButton(0) && distance_to_mouse > StopDistance)
        {
            transform.position += transform.right * Speed * Time.deltaTime;
        }

    }
}

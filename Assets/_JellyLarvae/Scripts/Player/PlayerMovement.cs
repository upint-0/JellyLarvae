using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Expandable]
    [SerializeField] private PlayerAttributesSO PlayerAttributes = null;
    
    [SerializeField] private JellyRenderer Jelly = null;

    [Header("Debug")] [SerializeField] private bool DebugMode = false;

    private Rigidbody2D _rigidbody2D = new Rigidbody2D();
    private SpriteRenderer _renderer = new SpriteRenderer();
    private Coroutine _dashRoutine = null;

    private Vector2 _direction = new Vector2();
    [Space] 
    [SerializeField] private PlayerEffect _playerEffect;

    private bool _inJelly;
    
    // Varying properties 
    [HideInInspector]public ValueWrapper<float> _CurrentMinSpeed;
    [HideInInspector]public ValueWrapper<float> _CurrentMaxSpeed;
    [HideInInspector]public ValueWrapper<float> _CurrentDashForce;

    public float CurrentMinSpeed => _CurrentMinSpeed.Value;
    public float CurrentMaxSpeed => _CurrentMaxSpeed.Value;
    public float CurrentDashForce => _CurrentDashForce.Value;

    private void Awake()
    {
        _CurrentMinSpeed = new ValueWrapper<float>(PlayerAttributes.MinSpeed);
        _CurrentMaxSpeed = new ValueWrapper<float>(PlayerAttributes.MaxSpeed);
        _CurrentDashForce = new ValueWrapper<float>(PlayerAttributes.DashForce);
    }

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        HandleDash();

        if (DetectJelly())
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
        _direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        float distance_to_mouse = _direction.magnitude;
        _direction = _direction.normalized;

        float z_rotation = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);

        HandleMovements(distance_to_mouse);
    }

    private void EnterJelly()
    {
        _rigidbody2D.gravityScale = 0.0f;
        if(!_inJelly)_playerEffect.EnterInJelly();

        _inJelly = true;
        if (DebugMode)
            _renderer.color = Color.green;
    }

    private void ExitJelly()
    {
        _rigidbody2D.gravityScale = PlayerAttributes.GravityScale;
        if(_inJelly)_playerEffect.ExitOutJelly();
        
        _inJelly = false;
        if (DebugMode)
            _renderer.color = Color.red;
    }

    private bool DetectJelly()
    {
        var position = transform.position;
        Debug.DrawRay(position - (transform.right * PlayerAttributes.EatOffset), Vector3.up, Color.blue);
        
        // :TODO: move this logic somewhere else
        bool eating = _rigidbody2D.velocity.magnitude > 1.0f;

        return (Jelly.GetJellyValueAtPosition( position, eating, position - (transform.right * PlayerAttributes.EatOffset), PlayerAttributes.EatRadius) > PlayerAttributes.JellyDetectionThreshold);
    }

    private void HandleDash()
    {
        if (Input.GetMouseButtonDown(1) && _dashRoutine == null)
        {
            _dashRoutine = StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        _rigidbody2D.AddForce(_direction * CurrentDashForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(PlayerAttributes.DashCoolDown);
        _dashRoutine = null;
    }

    private void HandleMovements(float distance_to_mouse)
    {
        if (Input.GetMouseButton(0) && distance_to_mouse > PlayerAttributes.StopDistance)
        {
            float speed = distance_to_mouse.Remap(
                PlayerAttributes.StopDistance,
                (Camera.main.orthographicSize * 2.0f)/4.0f,
                CurrentMinSpeed,
                CurrentMaxSpeed
                );
            
            _rigidbody2D.AddForce(_direction * (speed * Time.fixedDeltaTime), ForceMode2D.Force);
        }
    }
}
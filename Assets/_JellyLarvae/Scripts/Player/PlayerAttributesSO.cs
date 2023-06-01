using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttributes", menuName = "ScriptableObjects/PlayerAttributes", order = 1)]
public class PlayerAttributesSO : ScriptableObject
{
    // -- Fields
    
    [SerializeField] private float _minSpeed = 100.0f;
    [SerializeField] private float _maxSpeed = 200.0f;
    [SerializeField] private float _stopDistance = 1.0f;
    [SerializeField] private float _dashForce = 40.0f;
    [SerializeField] private float _dashCoolDown = 0.5f;
    [SerializeField] private float _gravityScale = 5.0f;
    
    // -- Porperties
    public float MinSpeed => _minSpeed;
    public float MaxSpeed => _maxSpeed;
    public float StopDistance => _stopDistance;
    public float DashForce => _dashForce;
    public float DashCoolDown => _dashCoolDown;
    public float GravityScale => _gravityScale;
}

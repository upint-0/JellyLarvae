using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttributes", menuName = "ScriptableObjects/PlayerAttributes", order = 1)]
public class PlayerAttributesSO : ScriptableObject
{
    // -- Fields
    
    [Header("Movements")]
    [SerializeField] private float _minSpeed = 100.0f;
    [SerializeField] private float _maxSpeed = 200.0f;
    [SerializeField] private float _stopDistance = 1.0f;
    [SerializeField] private float _dashForce = 40.0f;
    [SerializeField] private float _dashCoolDown = 0.5f;
    [SerializeField] private float _dashSlomoScale = 0.6f;
    [SerializeField] private float _dashSlomoTiming = 0.5f;
    [SerializeField] private float _gravityScale = 5.0f;
    [SerializeField] private float _eatOffset = 2.0f;
    [SerializeField] private float _eatRadius = 50.0f;
    [SerializeField] private float _jellyDetectionThreshold = 0.15f;

    [Header("Health")] 
    [SerializeField] private int _baseLevel = 1;


    // -- Porperties
    public float MinSpeed => _minSpeed;
    public float MaxSpeed => _maxSpeed;
    public float StopDistance => _stopDistance;
    public float DashForce => _dashForce;
    public float DashSlomoScale => _dashSlomoScale;
    public float DashSlomoTiming => _dashSlomoTiming;
    public float DashCoolDown => _dashCoolDown;
    public float GravityScale => _gravityScale;
    public float EatOffset => _eatOffset;
    public float EatRadius => _eatRadius;
    public int BaseLevel => _baseLevel;
    public float JellyDetectionThreshold => _jellyDetectionThreshold;
}

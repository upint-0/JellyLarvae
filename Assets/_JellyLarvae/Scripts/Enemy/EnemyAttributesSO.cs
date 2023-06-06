using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttributes", menuName = "ScriptableObjects/EnemyAttributes", order = 2)]
public class EnemyAttributesSO : ScriptableObject
{
    [Header("Health")]
    [SerializeField] private int _BaseLevel;
    [SerializeField] private int _Point;

    [Header("Movements")]
    [SerializeField] private float _Speed = 10f;
    [SerializeField] private float _AngularSpeed = 20f;

    [Header("Detection")] 
    [SerializeField, Range(0.5f, 20f)] private float _RangePlayerDetection = 4f;
    [SerializeField, Range(0.1f, 4f)] private float _RangeObstacleDetection = 1f;
    [SerializeField] private LayerMask _ObstaclesLayer;
    public int Level => _BaseLevel; 
    public int Point => _Point;
    
    public float Speed => _Speed;
    public float AngularSpeed => _AngularSpeed;
    public float RangePlayerDetection => _RangePlayerDetection;
    public float RangeObstacleDetection => _RangeObstacleDetection;
    public LayerMask ObstaclesLayer => _ObstaclesLayer;
}

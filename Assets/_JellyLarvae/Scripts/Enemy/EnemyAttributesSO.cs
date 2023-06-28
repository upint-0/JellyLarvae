using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyAttributes", menuName = "ScriptableObjects/EnemyAttributes", order = 2)]
public class EnemyAttributesSO : ScriptableObject
{
    [FormerlySerializedAs("minBaseMinLevel")]
    [FormerlySerializedAs("_BaseLevel")]
    [Header("Health")]
    [SerializeField] private int _MinBaseLevel;
    [SerializeField] private int _MaxBaseLevel;
    [SerializeField] private int _Point;
    [SerializeField] private int _Damage;
    [MinMaxSlider(0, 100)] public Vector2 _Rdm;
    
    [FormerlySerializedAs("minMinSpeed")]
    [FormerlySerializedAs("_Speed")]
    [Header("Movements")]
    [SerializeField] private float _MinSpeed = 450f;
    [SerializeField] private float _MaxSpeed = 550f;
    [SerializeField] private float _AngularSpeed = 20f;

    [Header("Detection")] 
    [SerializeField, Range(0.5f, 20f)] private float _RangePlayerDetection = 4f;
    [SerializeField, Range(0.1f, 4f)] private float _RangeObstacleDetection = 1f;
    [SerializeField] private LayerMask _ObstaclesLayer;
    [Space] 
    [SerializeField] private bool _IsEatingJelly = false;
    
    public int MinLevel => _MinBaseLevel; 
    public int MaxLevel => _MaxBaseLevel; 
    public int Point => _Point;
    public int Damage => _Damage;
    
    public float MinSpeed => _MinSpeed;
    public float MaxSpeed => _MaxSpeed;
    public float AngularSpeed => _AngularSpeed;
    public float RangePlayerDetection => _RangePlayerDetection;
    public float RangeObstacleDetection => _RangeObstacleDetection;
    public LayerMask ObstaclesLayer => _ObstaclesLayer;
    public bool IsEatingJelly => _IsEatingJelly;
}

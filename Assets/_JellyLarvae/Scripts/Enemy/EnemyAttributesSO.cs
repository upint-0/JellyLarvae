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
    public int Level => _BaseLevel; 
    public int Point => _Point;
    
    public float Speed => _Speed;
    public float AngularSpeed => _AngularSpeed;
}

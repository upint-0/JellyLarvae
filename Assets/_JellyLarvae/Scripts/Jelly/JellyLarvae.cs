using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class JellyLarvae : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _AngularSpeed = 5f;
    [SerializeField] private float _Speed = 300f;
    [SerializeField] private float _RangeObstacleDetection = 3f;
    [SerializeField] private LayerMask _ObstaclesLayer;

    [Header("Jelly Writer")] 
    [SerializeField] private float _WriteRadius = 10f;
    
    private Rigidbody2D _Rigidbody;
    private bool _AvoidObstacle;
    private Vector2 _CurrentDir;
    
    private void Awake()
    {
        _Rigidbody = GetComponent<Rigidbody2D>();
        FindNewRandomDir();
    }

    private void Start()
    {
        JellySurfaceDetection._Instance.AddJellyWriter(GetInstanceID(), transform.position, _WriteRadius);
    }

    private void OnDestroy()
    {
        JellySurfaceDetection._Instance.RemoveJellyWriter(GetInstanceID());
    }

    private void Update()
    {
        JellySurfaceDetection._Instance.UpdateJellyWriter(GetInstanceID(), transform.position);
    }

    private void FixedUpdate()
    {
        if(DetectObstacles()) FindNewRandomDir();
        HandleMovement(_CurrentDir);
    }

    private void FindNewRandomDir()
    {
        _CurrentDir = Random.insideUnitCircle.normalized;
    }

    private bool DetectObstacles()
    {
        Debug.DrawRay(transform.position,  (transform.right * _RangeObstacleDetection), Color.red);
        if (Physics2D.Raycast(transform.position, 
                transform.right, 
                _RangeObstacleDetection,
                _ObstaclesLayer))
        {
            return true;
        }

        return false;
    }

    
    private void HandleMovement(Vector2 targetDir)
    {
        Vector2 dir = Vector2.Lerp(transform.right, targetDir, _AngularSpeed * Time.fixedDeltaTime);
        
        float z_rotation = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, z_rotation);
        
        _Rigidbody.AddForce(transform.right * Time.fixedDeltaTime * _Speed, ForceMode2D.Force);
    }
}

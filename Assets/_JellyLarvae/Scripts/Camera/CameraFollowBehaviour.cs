using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent( typeof( Camera ) )]
public class CameraFollowBehaviour : MonoBehaviour
{
    public static CameraFollowBehaviour _Instance;
    [SerializeField] private Transform Target = null;
    [SerializeField] private float TargetOffset = 1.0f;
    [SerializeField] private float LerpSpeed = 1.0f;

    [Header( "Effect" )]
    [SerializeField, Range( 0.1f, 2f )] private float _ZoomEffectDuration = 0.2f;
    [SerializeField, Range( 0.1f, 2f )] private float _ZoomEffectScale = 5f;
    [SerializeField] private AnimationCurve _ZoomEffectCurve = new AnimationCurve( new Keyframe( 0, 0 ), new Keyframe( 1, 1 ) );
    [Space]
    [SerializeField, Range( 0.1f, 2f )] private float _ShakeEffectDuration = 0.2f;
    [SerializeField, Range( 0.1f, 2f )] private float _ShakeEffectScale = 5f;
    [SerializeField] private AnimationCurve _ShakeEffectCurve = new AnimationCurve( new Keyframe( 0, 0 ), new Keyframe( 1, 1 ) );

    private float _zoomTime;
    private float _shakeTime;
    private float _BaseCamScale;
    private float _CurrentCamScale;

    private Vector2 _targetPos = Vector2.zero;
    private Vector2 _EffectPos = Vector2.zero;

    private Coroutine _ZoomCoroutine;
    private Coroutine _ShakeCoroutine;
    private Camera _Cam;

    private Vector2 _previousCameraPos = Vector2.zero;

    [SerializeField] private float TEST = 1.0f;

    private void Awake()
    {
        if ( _Instance )
        {
            Destroy( this );
        }
        else
        {
            _Instance = this;
        }

        _Cam = GetComponent<Camera>();
        _BaseCamScale = _Cam.orthographicSize;
        _CurrentCamScale = _BaseCamScale;
    }

    private void Update()
    {
        if ( ( ( Vector2 ) transform.position - _previousCameraPos ).magnitude > TEST )
        {
            Shader.SetGlobalVector( "_CameraDeltaMovement", ( Vector2 ) transform.position );
            _previousCameraPos = transform.position;
        }
    }

    // :NOTE: FixedUpdate because the player movements is handled in FixedUpdate also
    private void FixedUpdate()
    {
        Vector2 target_position = ( Vector2 ) Target.position + ( Vector2 ) Target.right * TargetOffset;
        Vector2 lerped_position = Vector2.Lerp( transform.position, target_position, LerpSpeed * Time.fixedDeltaTime );
        transform.position = new Vector3( lerped_position.x, lerped_position.y, transform.position.z ) + _EffectPos.toVec3();
        _targetPos = target_position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere( _targetPos, 0.1f );
    }

    public void ImpulseZoom()
    {
        _ZoomCoroutine ??= StartCoroutine( ZoomCoroutine() );
    }
    public void PlayCameraShake()
    {
        _ShakeCoroutine ??= StartCoroutine( CameraShakeCoroutine() );
    }

    private IEnumerator ZoomCoroutine()
    {
        _zoomTime = _ZoomEffectDuration;
        while ( _zoomTime >= 0 )
        {
            _zoomTime -= Time.deltaTime;
            float percent = 1 - ( _zoomTime / _ZoomEffectDuration );
            _CurrentCamScale = Mathf.Max( 0.1f, _BaseCamScale - _ZoomEffectScale * _ZoomEffectCurve.Evaluate( percent ) );
            _Cam.orthographicSize = _CurrentCamScale;
            yield return null;
        }

        _ZoomCoroutine = null;
    }

    private IEnumerator CameraShakeCoroutine()
    {
        _shakeTime = _ShakeEffectDuration;
        while ( _shakeTime >= 0 )
        {
            _shakeTime -= Time.deltaTime;
            float percent = 1 - ( _shakeTime / _ShakeEffectDuration );
            _EffectPos = Random.insideUnitCircle * _ShakeEffectScale * _ShakeEffectCurve.Evaluate( percent );
            _Cam.orthographicSize = _CurrentCamScale;
            yield return null;
        }

        _ShakeCoroutine = null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowBehaviour : MonoBehaviour
{
    [SerializeField] private Transform Target = null;
    [SerializeField] private float TargetOffset = 1.0f;
    [SerializeField] private float LerpSpeed = 1.0f;

    private Vector2 _targetPos = Vector2.zero;

    // :NOTE: FixedUpdate because the player movements is handled in FixedUpdate also
    private void FixedUpdate()
    {
        Vector2 target_position = (Vector2) Target.position + (Vector2) Target.right * TargetOffset;
        Vector2 lerped_position = Vector2.Lerp( transform.position, target_position, LerpSpeed * Time.fixedDeltaTime );
        transform.position = new Vector3( lerped_position.x, lerped_position.y, transform.position.z);
        _targetPos = target_position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere( _targetPos, 0.1f );
    }
}

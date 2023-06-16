using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Transform Eyes = null;

    private void Update()
    {
        Vector2 direction = Camera.main.ScreenToWorldPoint( Input.mousePosition ) - transform.position;
        float rotation_z = Mathf.Atan2( direction.y, direction.x ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler( 0, 0, rotation_z - 90 );
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer PlayerSprite = null;
    [SerializeField] private Transform Eyes = null;
    [SerializeField] private TrailRenderer PlayerTrail = null;
    [SerializeField] private ParticleSystem DashReloadedEffect = null;

    [SerializeField] private Color PlayerColor = Color.white;
    [SerializeField] private Color PlayerCooldownColor = Color.black;

    private PlayerMovement PlayerMovement = null;

    private void EyesRotation()
    {
        Vector2 direction = Camera.main.ScreenToWorldPoint( Input.mousePosition ) - transform.position;
        float rotation_z = Mathf.Atan2( direction.y, direction.x ) * Mathf.Rad2Deg;

        Eyes.rotation = Quaternion.Euler( 0, 0, rotation_z - 90 );
    }

    private void DashAnimation(bool start_dash)
    {
        print( "event called" );
        if ( start_dash )
        {
            ChangePlayerColor( PlayerCooldownColor );
        }
        else
        {
            ChangePlayerColor( PlayerColor );
            DashReloadedEffect.Play();
        }
    }

    private void ChangePlayerColor( Color new_color )
    {
        PlayerSprite.color = new_color;
        PlayerTrail.startColor = new_color;
        PlayerTrail.endColor = new_color;
    }

    #region Unity

    private void Awake()
    {
        PlayerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        PlayerMovement.OnDash.AddListener( DashAnimation );
    }

    private void OnDestroy()
    {
        PlayerMovement.OnDash.RemoveListener( DashAnimation );
    }

    private void Update()
    {
        //EyesRotation();
    }

    #endregion

}

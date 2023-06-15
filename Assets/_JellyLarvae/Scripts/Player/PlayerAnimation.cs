using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Transform Eyes = null;

    private void Update()
    {
        Eyes.LookAt( Camera.main.ScreenToWorldPoint( Input.mousePosition ) );
        //Eyes.rota
    }
}

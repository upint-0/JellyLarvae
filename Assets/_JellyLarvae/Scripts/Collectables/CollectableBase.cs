using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableBase : MonoBehaviour
{
    public int _TypeID;
    protected virtual void Collect()
    {
        CollectableManager._Instance.RemoveCollectable(_TypeID);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")) Collect();
    }
}

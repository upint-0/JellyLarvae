using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerHealthNetwork : NetworkBehaviour
{
    public NetworkVariable<int> _Level = new(1, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    [SerializeField] private TMP_Text _LevelText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer)_Level.OnValueChanged += OnLevelChanged;
    }

    private void OnLevelChanged(int previousvalue, int newvalue)
    {
        Debug.Log("Level changed!");
        _LevelText.text = newvalue.ToString();
    }

    // Call by the server 
    [ContextMenu("--- Add Level ---")]
    public void AddLevel()
    {
        _Level.Value += 1;
    }
    
}

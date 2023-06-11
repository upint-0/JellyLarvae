using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _TextPlayerLevel;

    private PlayerEntity _PlayerRef;

    private void Start()
    {
        _PlayerRef = GameManager._Instance._Player;
    }

    private void Update()
    {
        _TextPlayerLevel.text = "Level : " + _PlayerRef.CurrentLevel;
    }
}

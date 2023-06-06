using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerEffect : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip[] _EnterIntoJellySounds;

    private const float _MinInteraval = 0.25f;
    private float _TimeInterval;

    public void EnterInJelly()
    {
        PlaySound(_EnterIntoJellySounds[Random.Range(0, _EnterIntoJellySounds.Length)]);
    }
    public void ExitOutJelly()
    {
        PlaySound(_EnterIntoJellySounds[Random.Range(0, _EnterIntoJellySounds.Length)]);
    }

    private void Update()
    {
        _TimeInterval += Time.deltaTime;
    }

    private void PlaySound(AudioClip audio)
    {
        if (_TimeInterval <= _MinInteraval) return;
        _TimeInterval = 0f;
        
        AudioSource.PlayClipAtPoint(audio, transform.position);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerEffect : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private AudioClip[] _EnterIntoJellySounds;
    [SerializeField] private AudioClip[] _EatSounds;
    [SerializeField] private AudioClip[] _DamageSounds;

    private const float _MinInteraval = 0.25f;
    private float[] _TimeInterval;

    private void Awake()
    {
        _TimeInterval = new float[3];
    }

    public void EnterInJelly()
    {
        PlaySound(_EnterIntoJellySounds[Random.Range(0, _EnterIntoJellySounds.Length)],0);
    }
    public void ExitOutJelly()
    {
        PlaySound(_EnterIntoJellySounds[Random.Range(0, _EnterIntoJellySounds.Length)],0);
    }
    
    public void Eat()
    {
        PlaySound(_EatSounds[Random.Range(0, _EatSounds.Length)],1);
    }
    public void Damaged()
    {
        PlaySound(_DamageSounds[Random.Range(0, _DamageSounds.Length)],2);
    }


    private void Update()
    {
        for (int i = 0; i < _TimeInterval.Length; i++)
        {
            _TimeInterval[i] += Time.deltaTime;
        }
    }

    private void PlaySound(AudioClip audio, int index)
    {
        if (_TimeInterval[index] <= _MinInteraval) return;
        _TimeInterval[index] = 0f;
        
        AudioSource.PlayClipAtPoint(audio, transform.position);
    }
}

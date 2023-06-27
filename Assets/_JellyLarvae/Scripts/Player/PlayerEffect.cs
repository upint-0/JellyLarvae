using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum ScreenEffectType
{
    Speed,
    Dash,
    Invisibility
}
[Serializable]
public struct ScreenEffect
{
    public PropertyBonus.E_BonusType _Effect;
    public Color _EffectColor;
    [Range(0,1)]public float _VignettePercent;
    public AnimationCurve _CurveEffect;
    [Space, Range(0,1)] public float _ChromaticAberrationPercent;
    public AnimationCurve _CurveChromaticEffect;
    [HideInInspector] public float _MaxTime;
    [HideInInspector] public float _CurrentTime;
}
public class PlayerEffect : MonoBehaviour
{
    [Header("Particle System")] 
    [SerializeField] private ParticleSystem _EatEffect;

    [Header("Screen Effect")] 
    [SerializeField] private Volume _PostProcessVolume; 
    [SerializeField] private ScreenEffect[] _ScreenEffects;
    private VolumeProfile _GlobalPP;
    private Vignette _VignettePP;
    private ChromaticAberration _ChromaticPP;
    private bool _ScreenEffectActive;
    private Coroutine _CoroutineScreenEffect;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] _EnterIntoJellySounds;
    [SerializeField] private AudioClip[] _EatSounds;
    [SerializeField] private AudioClip[] _DamageSounds;

    private const float _MinInteraval = 0.25f;
    private float[] _TimeInterval;

    private void Awake()
    {
        _TimeInterval = new float[3];
        _GlobalPP = _PostProcessVolume.profile;
        _GlobalPP.TryGet(out _VignettePP);
        _GlobalPP.TryGet(out _ChromaticPP);
    }

    public void EnterInJelly()
    {
        _EatEffect.Play();
        PlaySound(_EnterIntoJellySounds[Random.Range(0, _EnterIntoJellySounds.Length)],0);
    }
    public void ExitOutJelly()
    {
        _EatEffect.Stop();
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

    public void StartScreenEffect(PropertyBonus.E_BonusType type, float time)
    {
        int index = GetScreenEffectIndex(type);

        _ScreenEffects[index]._MaxTime = time;
        _ScreenEffects[index]._CurrentTime = time;

        _ScreenEffectActive = true;
        _CoroutineScreenEffect ??= StartCoroutine(UpdateScreenEffect());
    }

    private IEnumerator UpdateScreenEffect()
    {
        while (_ScreenEffectActive)
        {
            float maxTime = 0f;
            Color color = new Color(0,0,0,0);
            int screenEffectCounter = 0;
            float currentPercentEffect = 0f;
            float currentChromaticPercentEffect = 0f;

            for (int i = 0; i < _ScreenEffects.Length; i++)
            {
                if (_ScreenEffects[i]._CurrentTime > 0f)
                {
                    screenEffectCounter++;
                    _ScreenEffects[i]._CurrentTime -= Time.deltaTime;
                    float percent = _ScreenEffects[i]._CurrentTime / _ScreenEffects[i]._MaxTime;
                    //_VignettePP.intensity.value = _ScreenEffects[i]._CurveEffect.Evaluate(1 - percent) * _ScreenEffects[i]._VignettePercent;
                    //_VignettePP.color.value = _ScreenEffects[i]._EffectColor;

                    currentPercentEffect += _ScreenEffects[i]._CurveEffect.Evaluate(1 - percent) *
                                            _ScreenEffects[i]._VignettePercent;
                    currentChromaticPercentEffect += _ScreenEffects[i]._CurveChromaticEffect.Evaluate(1 - percent) *
                                            _ScreenEffects[i]._ChromaticAberrationPercent;
                    color += _ScreenEffects[i]._EffectColor;
                    if (_ScreenEffects[i]._CurrentTime > maxTime)
                    {
                        maxTime = _ScreenEffects[i]._CurrentTime;
                    }
                }
            }
            
            _VignettePP.color.value = color / screenEffectCounter;
            _VignettePP.intensity.value = currentPercentEffect / screenEffectCounter;
            _ChromaticPP.intensity.value = currentChromaticPercentEffect / screenEffectCounter;

            if (maxTime <= 0f)
            {
                _ScreenEffectActive = false;
                _CoroutineScreenEffect = null;
            }
            yield return new WaitForEndOfFrame(); 
        }
    }

    private int GetScreenEffectIndex(PropertyBonus.E_BonusType type)
    {
        for (int i = 0; i < _ScreenEffects.Length; i++)
        {
            if (_ScreenEffects[i]._Effect == type) return i;
        }
        return 0;
    }
}

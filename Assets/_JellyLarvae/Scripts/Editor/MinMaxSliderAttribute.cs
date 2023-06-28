using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMaxSliderAttribute : PropertyAttribute
{
    public float _Min, _Max;

    public MinMaxSliderAttribute(float min, float max)
    {
        this._Min = min;
        this._Max = max;
    }
}

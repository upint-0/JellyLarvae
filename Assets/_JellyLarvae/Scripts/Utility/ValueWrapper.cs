using System;
using UnityEngine;

[Serializable]
public class ValueWrapper<T>
{
    [SerializeField] private T value;
    private T baseValue;

    public T Value
    {
        get { return value; }
        set { this.value = value; }
    }

    public T BaseValue
    {
        get { return baseValue; }
        set { baseValue = value; }
    }
    
    public ValueWrapper(T value)
    {
        Value = value;
        BaseValue = Value;
    }
}
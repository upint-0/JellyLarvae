using System;

[Serializable]
public class ValueWrapper<T>
{
    public T Value { get; set;  }
    public T BaseValue { get; set;  }

    public ValueWrapper(T value)
    {
        Value = value;
        BaseValue = Value;
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiInterpolator<TValue>
{
    private List<KeyValuePair<float, TValue>> _keyValuePairs;

    public Func<TValue, TValue, float, TValue> LerpFunction { private get; set; }
    public Func<float, float> EaseFunction { private get; set; }

    public MultiInterpolator(
        IEnumerable<KeyValuePair<float, TValue>> keyValuePairs,
        Func<TValue, TValue, float, TValue> lerpFunction,
        Func<float, float> easeFunction = null)
    {
        if (keyValuePairs == null)
            throw new ArgumentNullException(nameof(keyValuePairs));

        _keyValuePairs = new List<KeyValuePair<float, TValue>>(keyValuePairs);

        if (_keyValuePairs.Count < 2)
            throw new ArgumentException("keyValuePairs must have at least two elements.");

        _keyValuePairs.Sort((a, b) => a.Key.CompareTo(b.Key));

        LerpFunction = lerpFunction ?? throw new ArgumentNullException(nameof(lerpFunction));
        EaseFunction = easeFunction ?? (t => t);
    }

    public TValue Evaluate(float x)
    {
        int count = _keyValuePairs.Count;

        if (x <= _keyValuePairs[0].Key) return _keyValuePairs[0].Value;
        if (x >= _keyValuePairs[count - 1].Key) return _keyValuePairs[count - 1].Value;

        for (int i = 0; i < count - 1; i++)
        {
            float kl = _keyValuePairs[i].Key;
            float kr = _keyValuePairs[i + 1].Key;

            if (x >= kl && x <= kr)
            {
                float t = Mathf.InverseLerp(kl, kr, x);
                t = EaseFunction(t);
                return LerpFunction(_keyValuePairs[i].Value, _keyValuePairs[i + 1].Value, t);
            }
        }

        return _keyValuePairs[count - 1].Value;
    }

    /// <summary>
    /// Add a new key-value pair. The list is sorted after insertion.
    /// </summary>
    public void AddPair(float key, TValue value)
    {
        _keyValuePairs.Add(new KeyValuePair<float, TValue>(key, value));
        _keyValuePairs.Sort((a, b) => a.Key.CompareTo(b.Key));
    }

    /// <summary>
    /// Remove a key-value pair by key. Returns true if removed.
    /// </summary>
    public bool RemovePair(float key)
    {
        int index = _keyValuePairs.FindIndex(kv => Mathf.Approximately(kv.Key, key));
        if (index >= 0)
        {
            _keyValuePairs.RemoveAt(index);
            return true;
        }
        return false;
    }

    public void SetPairs(List<KeyValuePair<float, TValue>> keyValuePairs)
    {
        _keyValuePairs = keyValuePairs;
        _keyValuePairs.Sort((a, b) => a.Key.CompareTo(b.Key));
    }

    public void SetPairs(IEnumerable<KeyValuePair<float, TValue>> keyValuePairs)
    {
        SetPairs(new List<KeyValuePair<float, TValue>>(keyValuePairs));
    }
}

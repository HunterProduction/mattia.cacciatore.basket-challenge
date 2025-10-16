using System;
using UnityEngine;

[Serializable]
public class Bonus
{
    public enum ApplyType
    {
        Additive,
        Multiplicative
    }

    [SerializeField] protected string id;
    public string Id => id;

    [SerializeField] protected float value;
    public float Value => value;

    [SerializeField] protected ApplyType type;
    public ApplyType Type => type;

    [Tooltip("Time in seconds. If 0, bonus does not expire with time.")]
    [SerializeField] protected int expiresIn;
    public float ExpiresIn => expiresIn;

    public Bonus(float bonusValue, ApplyType bonusType, int expiresIn, string id = "")
    {
        this.id = string.IsNullOrWhiteSpace(id) ?
            $"Bonus_{Guid.NewGuid()}" :
            $"{id}_{Guid.NewGuid()}";

        this.value = bonusValue;
        this.type = bonusType;
        this.expiresIn = expiresIn;
    }

    public virtual void ApplyBonus(ref int score)
    {
        switch (type)
        {
            case ApplyType.Additive:
                score += (int)value;
                break;
            case ApplyType.Multiplicative:
                score = (int)(score * value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

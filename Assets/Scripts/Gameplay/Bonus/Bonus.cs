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

    public Bonus(float bonusValue, ApplyType bonusType, string id = "")
    {
        if(string.IsNullOrWhiteSpace(id))
            id = "ScoreBonus_"+Guid.NewGuid().ToString();

        this.id = id;
        this.value = bonusValue;
        this.type = bonusType;
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

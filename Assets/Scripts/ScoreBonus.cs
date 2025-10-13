using System;
using UnityEngine;

[Serializable]
public struct ScoreBonus
{
    public enum Type
    {
        Additive,
        Multiplicative
    }

    [SerializeField] private string id;
    public float value;
    public Type type;
    public EventRarity rarity;

    public readonly string Id => id;

    public ScoreBonus(float bonusValue, Type bonusType, EventRarity rarity, string id = "")
    {
        if(string.IsNullOrWhiteSpace(id))
            id = "ScoreBonus_"+Guid.NewGuid().ToString();

        this.id = id;
        this.value = bonusValue;
        this.type = bonusType;
        this.rarity = rarity;
    }

    public void ApplyBonus(ref int score)
    {
        switch (type)
        {
            case Type.Additive:
                score += (int)value;
                break;
            case Type.Multiplicative:
                score = (int)(score * value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

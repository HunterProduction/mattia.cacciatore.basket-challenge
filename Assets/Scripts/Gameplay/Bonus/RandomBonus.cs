using System;
using UnityEngine;

[Serializable]
public class RandomBonus : Bonus
{
    [SerializeField] protected EventRarity rarity;
    public EventRarity Rarity => rarity;   

    public RandomBonus(float bonusValue, ApplyType bonusType, EventRarity rarity, string id = "") : base(bonusValue, bonusType, id)
    {
        this.rarity = rarity;
    }
}

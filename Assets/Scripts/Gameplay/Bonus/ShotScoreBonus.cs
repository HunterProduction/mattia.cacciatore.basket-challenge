using System;
using UnityEngine;

[Serializable]
public class ShotScoreBonus : RandomBonus
{
    [SerializeField]
    private ShotType appliedTo;

    public ShotScoreBonus(float bonusValue, ApplyType bonusType, int expiresIn, EventRarity rarity, ShotType appliedTo, string id = "") : base(bonusValue, bonusType, expiresIn, rarity, id)
    {
        this.appliedTo = appliedTo;
    }

    public bool IsAppliedTo(ShotType shotType)
    {
        return appliedTo == shotType;
    }
}

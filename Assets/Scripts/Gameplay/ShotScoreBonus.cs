using System;
using UnityEngine;

[Serializable]
public class ShotScoreBonus : Bonus
{
    [SerializeField]
    private ShotType _appliedTo;

    public ShotScoreBonus(float bonusValue, ApplyType bonusType, EventRarity rarity, ShotType appliedTo, string id = "") : base(bonusValue, bonusType, rarity, id)
    {
        this._appliedTo = appliedTo;
    }

    public bool IsAppliedTo(ShotType shotType)
    {
        return _appliedTo == shotType;
    }
}

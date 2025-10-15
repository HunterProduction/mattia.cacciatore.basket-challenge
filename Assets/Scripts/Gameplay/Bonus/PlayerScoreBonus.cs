using System;
using UnityEngine;

[Serializable]
public class PlayerScoreBonus : Bonus
{
    [SerializeField] private BasketballPlayer player;
    public BasketballPlayer Player => player;

    public PlayerScoreBonus(BasketballPlayer player, float bonusValue, ApplyType bonusType, EventRarity rarity, string id = "") : base(bonusValue, bonusType, id)
    {
        this.player = player;
    }
}

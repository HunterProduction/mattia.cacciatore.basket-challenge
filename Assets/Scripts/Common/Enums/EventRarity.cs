using System;
using UnityEngine;

public enum EventRarity
{
    Common,
    Uncommon,
    Rare,
}

public static class EventRarityExtensions
{
    /**
     * #NOTE: #MattiaCacciatore This mapping has been hard-coded due to time reasons.
     * It could easily be updated to a configurable data-driven solution (e.g. using ScriptableObjects).
     */
    public static Color GetColor(this EventRarity rarity)
    {
        return rarity switch
        {
            EventRarity.Common => Color.green,
            EventRarity.Uncommon => Color.cyan,
            EventRarity.Rare => Color.magenta,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public static float GetFrequencyPerSecond(this EventRarity rarity)
    {
        var config = BasketballGameManager.Instance.GameConfigs;

        return rarity switch
        {
            EventRarity.Common =>   config.commonEventFrequencyPerSecond,
            EventRarity.Uncommon => config.uncommonEventFrequencyPerSecond,        
            EventRarity.Rare =>     config.rareEventFrequencyPerSecond,        
            _ => throw new ArgumentOutOfRangeException(),
        };
    }  
}
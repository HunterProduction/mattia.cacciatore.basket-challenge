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
     * #NOTE: #MattiaCacciatore These mappings have been hard-coded due to time reasons.
     * They could easily be updated to a configurable data-driven solution (e.g. using ScriptableObjects).
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
        return rarity switch
        {
            EventRarity.Common =>   1 / 10f,        // One event every 10 seconds.
            EventRarity.Uncommon => 1 / 20f,        // One event every 20 seconds.
            EventRarity.Rare =>     1 / 40f,        // One event every 40 seconds.
            _ => throw new ArgumentOutOfRangeException(),
        };
    }  
}
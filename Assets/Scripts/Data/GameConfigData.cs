using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigData", menuName = "Basketball/Game Config Data", order = 0)]
public class GameConfigData : ScriptableObject
{
    [Header("Score")]
    public int perfectShotScore = 5;
    public int ringShotScore = 2;
    public int backboardShotScore = 2;

    [Header("Time")]
    public float gameDuration = 120f;

    [Header("Probabilities")]
    public float commonEventFrequencyPerSecond =    1 / 10f;    // One event every 10 seconds.
    public float uncommonEventFrequencyPerSecond =  1 / 20f;    // One event every 20 seconds.
    public float rareEventFrequencyPerSecond =      1 / 40f;    // One event every 40 seconds.
}

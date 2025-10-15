using UnityEngine;

[CreateAssetMenu(fileName = "AiBasketballPlayerData", menuName = "Basketball/Ai BaskeballPlayer Data", order = 0)]
public class AiBasketballPlayerData : ScriptableObject
{
    [Tooltip("Time in seconds the AI takes to aim before shooting.")]
    public float aimTime = 3f;

    [Tooltip("Variance percentage to apply to the aim time (e.g. 0.2 means +/- 20% variance)")]
    [Range(0f, 1f)]
    public float aimTimeVariancePercentage = 0.2f;

    [Tooltip("Accuracy of the AI player. 1 means it'll always shoot with perfect initial velocity")]
    [Range(0f, 1f)]
    public float accuracy = 0.8f;

    [Tooltip("Preference of the AI player for perfect shots vs backboard shots. 0 means it'll always aim at perfect shots, 1 at backboard shots")]
    [Range(0f, 1f)]
    public float perfectOrBackboardShotPreference = 0.5f;
}

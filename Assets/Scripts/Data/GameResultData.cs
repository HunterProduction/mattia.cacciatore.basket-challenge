using UnityEngine;

[CreateAssetMenu(fileName = "GameResultData", menuName = "Basketball/Game Result Data", order = 1)]
public class GameResultData : ScriptableObject
{
    public MatchResult matchResult;

    // #TODO: #MattiaCacciatore Add more data as needed (e.g. stats, scores, etc).
}

using TMPro;
using UnityEngine;

public class UIEndGamePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;
    public UIPlayerScoreCounter[] playerScoreCounters;

    public void SetResultText(MatchResult matchResult)
    {
        resultText.text = matchResult switch
        {
            MatchResult.Win => "WIN!",
            MatchResult.Lose => "LOSE",
            MatchResult.Draw => "DRAW!",
            _ => throw new System.ArgumentException()
        };
    }
}

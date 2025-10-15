using TMPro;
using UnityEngine;

public class UIEndGamePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;
    public UIPlayerScoreCounter[] playerScoreCounters;

    public void SetResultText(MatchResult matchResult)
    {
        resultText.text = matchResult == MatchResult.Win ? " WIN!" : " LOSE";
    }
}

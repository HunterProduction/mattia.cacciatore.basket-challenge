using TMPro;
using UnityEngine;

public class UIEndGamePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;
    public UIPlayerScoreCounter scoreCounterPlayer1, scoreCounterPlayer2;

    public void SetResultText(MatchResult matchResult)
    {
        resultText.text = matchResult == MatchResult.Win ? " WIN!" : " LOSE";
    }
}

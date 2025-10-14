using TMPro;
using UnityEngine;

public class UIGameResultPanel : MonoBehaviour
{
    [SerializeField] private GameResultData gameResultData;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI matchResultLabel;
    [SerializeField] private UIPlayerStats player1Stats, player2Stats;
 
    private void OnEnable()
    {
        if(gameResultData == null)
        {
            Debug.LogError($"[{GetType().Name}] Missing GameResultData reference.");
            return;
        }

        matchResultLabel.text = gameResultData.matchResult == MatchResult.Win ? "You Win!" : "You Lose";

        var stats = gameResultData.GetAllPlayerStats(true);

        player1Stats.Set(stats[0]);
        if(stats.Length > 1)
        {
            player2Stats.gameObject.SetActive(true);
            player2Stats.Set(stats[1]);
        }
        else
        {
            player2Stats.gameObject.SetActive(false);
        }
    }
}

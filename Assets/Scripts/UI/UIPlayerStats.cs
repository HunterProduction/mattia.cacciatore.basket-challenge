using TMPro;
using UnityEngine;

public class UIPlayerStats : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerIdText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Set(PlayerStats playerStats)
    {
        playerIdText.text = playerStats.Id;
        scoreText.text = playerStats.score.ToString();
    }
}

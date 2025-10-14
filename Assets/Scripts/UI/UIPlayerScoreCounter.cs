using System;
using TMPro;
using UnityEngine;

public class UIPlayerScoreCounter : MonoBehaviour
{
    [SerializeField] private BasketballPlayer player;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerIdText;
    [SerializeField] private TextMeshProUGUI scoreText;

    private BasketballGameManager _gameManager;

    public BasketballPlayer Player
    {
        get => player;
        set
        {
            player = value;
            if(player != null)
            {
                scoreText.text = BasketballGameManager.Instance.GetPlayerScore(player).ToString();
                if (playerIdText != null)
                {
                    playerIdText.text = player.Id;
                }
            }            
        }
    }

    private void OnEnable()
    {
        _gameManager = BasketballGameManager.Instance;

        _gameManager.onPointScored.AddListener(UpdateText);
    }

    private void UpdateText(PointScoredArgs pointScoredArgs)
    {
        if(pointScoredArgs.shotData.player != player)
            return;
        scoreText.text = _gameManager.GetPlayerScore(player).ToString();
    }

    private void OnDisable()
    {
        if (_gameManager != null)
            _gameManager.onPointScored?.RemoveListener(UpdateText);
    }
}

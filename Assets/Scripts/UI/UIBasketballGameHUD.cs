using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBasketballGameHUD : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private UIVelocityProgressBar velocityProgressBar;
    [SerializeField] private UIPlayerScoreCounter scoreCounterPlayer1, scoreCounterPlayer2;
    [SerializeField] private UIEndGamePopup endGamePopup;

    private BasketballGameManager _gameManager;

    private void Awake()
    {
        velocityProgressBar.gameObject.SetActive(false);
        endGamePopup.gameObject.SetActive(false);
    }

    private void Start()
    {
        _gameManager = BasketballGameManager.Instance;

        _gameManager.onGameStarted.AddListener(OnGameStarted);
        _gameManager.onGameOver.AddListener(OnGameOver);

        var players = _gameManager.Players;
        scoreCounterPlayer1.player = players[0];
        if(players.Length > 1)
            scoreCounterPlayer2.player = players[1];
    }

    private void OnGameOver(MatchResult result)
    {
        velocityProgressBar.gameObject.SetActive(false);

        endGamePopup.scoreCounterPlayer1.player = scoreCounterPlayer1.player;
        endGamePopup.scoreCounterPlayer2.player = scoreCounterPlayer2.player;
        endGamePopup.SetResultText(result);
        endGamePopup.gameObject.SetActive(true);
    }

    private void OnGameStarted()
    {
        velocityProgressBar.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if(_gameManager != null)
        {
            _gameManager.onGameStarted?.RemoveListener(OnGameStarted);
            _gameManager.onGameOver?.RemoveListener(OnGameOver);
        }
    }
}

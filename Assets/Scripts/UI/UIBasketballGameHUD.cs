using UnityEngine;

public class UIBasketballGameHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIVelocityProgressBar velocityProgressBar;
    [SerializeField] private UIPlayerScoreCounter scoreCounterPlayer1, scoreCounterPlayer2;
    [SerializeField] private UIEndGamePopup endGamePopup;

    private BasketballGameManager _gameManager;

    private void Start()
    {
        _gameManager = BasketballGameManager.Instance;

        _gameManager.onGameStarted.AddListener(OnGameStarted);
        _gameManager.onGameOver.AddListener(OnGameOver);

        var players = _gameManager.Players;
        scoreCounterPlayer1.Player = players[0];
        if(players.Length > 1)
            scoreCounterPlayer2.Player = players[1];

        velocityProgressBar.gameObject.SetActive(false);
        endGamePopup.gameObject.SetActive(false);
        scoreCounterPlayer1.gameObject.SetActive(false);
        scoreCounterPlayer2.gameObject.SetActive(false);
    }

    private void OnGameOver(MatchResult result)
    {
        velocityProgressBar.gameObject.SetActive(false);
        scoreCounterPlayer1.gameObject.SetActive(false);
        scoreCounterPlayer2.gameObject.SetActive(false);

        endGamePopup.scoreCounterPlayer1.Player = scoreCounterPlayer1.Player;
        endGamePopup.scoreCounterPlayer2.Player = scoreCounterPlayer2.Player;
        endGamePopup.SetResultText(result);
        endGamePopup.gameObject.SetActive(true);
    }

    private void OnGameStarted()
    {
        velocityProgressBar.gameObject.SetActive(true);
        scoreCounterPlayer1.gameObject.SetActive(true);
        scoreCounterPlayer2.gameObject.SetActive(true);
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

using UnityEngine;

public class UIBasketballGameHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIVelocityProgressBar velocityProgressBar;
    [SerializeField] private UIPlayerScoreCounter[] playerScoreCounters;
    [SerializeField] private UIEndGamePopup endGamePopup;
    [SerializeField] private UIFireballProgressBar fireballProgressBar;

    private BasketballGameManager _gameManager;

    private void Start()
    {
        _gameManager = BasketballGameManager.Instance;

        _gameManager.onGameStarted.AddListener(OnGameStarted);
        _gameManager.onGameOver.AddListener(OnGameOver);

        InitializeScoreCounters();

        endGamePopup.gameObject.SetActive(false);
        ToggleHUD(false);   
    }

    private void InitializeScoreCounters()
    {
        if (playerScoreCounters.Length <= 0)
            return;

        // Set the first score counter to the user player.
        playerScoreCounters[0].Player = _gameManager.GetUserPlayer();

        var players = _gameManager.Players;
        if(players.Length != playerScoreCounters.Length)
        {
            Debug.LogError($"[{GetType().Name}] Number of players is different than number of score counters", this);
            return;
        }

        // Set the rest of the score counters to the non-user players.
        int i = 1;
        foreach (var player in players)
        {
            if (!player.IsUser)
            {
                playerScoreCounters[i].Player = player;
            }
        }
    }

    private void OnGameOver(GameOverArgs result)
    {
        ToggleHUD(false);   

        if(endGamePopup.playerScoreCounters.Length != playerScoreCounters.Length)
        {
            Debug.LogError($"[{GetType().Name}] Number of score counters is different than number of end game score counters", endGamePopup);
            return;
        }

        for(int i = 0; i < playerScoreCounters.Length; i++)
        {
            endGamePopup.playerScoreCounters[i].Player = playerScoreCounters[i].Player;
        }
        endGamePopup.SetResultText(result.matchResult);
        endGamePopup.gameObject.SetActive(true);
    }

    private void OnGameStarted()
    {
        ToggleHUD(true);
    }

    private void ToggleHUD(bool enabled)
    {
        velocityProgressBar.gameObject.SetActive(enabled);
        fireballProgressBar.gameObject.SetActive(enabled);
        foreach (var scoreCounter in playerScoreCounters)
        {
            scoreCounter.gameObject.SetActive(enabled);
        }
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

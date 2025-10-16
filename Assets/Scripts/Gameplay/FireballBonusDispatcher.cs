using UnityEngine;
using UnityEngine.Events;

public class FireballBonusDispatcher : MonoBehaviour
{
    [SerializeField] private GameConfigData gameConfig;
    [SerializeField] private BasketballPlayer player;

    [Header("Unity Events")]
    public UnityEvent onFireballBonusStarted;
    public UnityEvent onFireballBonusReset;

    private PlayerScoreBonus _bonus;
    private BasketballGameManager _gameManager;
    private int _streakCount;
    private bool _isActive;
    private BasketballBonusManager _bonusManager;

    public int CurrentStreak => _streakCount;
    public int Threshold => gameConfig.fireballBonusStreakThreshold;

    private void Awake()
    {
        if(player == null)
        {
            Debug.LogError($"{GetType().Name} No player assigned to bonus.");
            return;
        }

        _gameManager = BasketballGameManager.Instance;
        _bonusManager = BasketballBonusManager.Instance;

        _gameManager.onPointScored.AddListener(OnPointScored);
        _gameManager.onShotMiss.AddListener(OnShotMiss);

        _bonus = new PlayerScoreBonus(
            player,
            gameConfig.fireballBonus.Value,
            gameConfig.fireballBonus.Type,
            gameConfig.fireballBonus.ExpiresIn,
            $"{gameConfig.fireballBonus.Id}_{player.name}"
            );

        _bonusManager.bonusRemoved += OnBonusRemoved;
    }

    private void OnShotMiss(BasketballPlayer missPlayer)
    {
        if (missPlayer != player)
            return;

        ResetBonus();
    }

    private void OnPointScored(PointScoredArgs pointScoredArgs)
    {
        if (_isActive || pointScoredArgs.shotData.player != player)
            return;

        _streakCount++;
        Debug.Log($"{GetType().Name} Player {player.name} streak: {_streakCount}");
        if(_streakCount >= gameConfig.fireballBonusStreakThreshold)
        {
            Debug.Log($"{GetType().Name} Player {player.name} bonus started!");
            onFireballBonusStarted?.Invoke();
            _bonusManager.AddBonus(_bonus);
            _isActive = true;            
        }
    }

    private void OnBonusRemoved(string bonusId)
    {
        if(bonusId == _bonus.Id)
        {
            ResetBonus();
        }
    }

    private void ResetBonus()
    {
        if (_isActive)
        {
            _streakCount = 0;
            _isActive = false;
            _bonusManager.RemoveBonus(_bonus);
            onFireballBonusReset?.Invoke();
            Debug.Log($"{GetType().Name} Player {player.name} bonus reset");
        }
        else
        {
            _streakCount = 0;
            Debug.Log($"{GetType().Name} Player {player.name} streak reset: {_streakCount}");
        }
    }

    private void OnDestroy()
    {
        onFireballBonusReset.RemoveAllListeners();
        onFireballBonusStarted.RemoveAllListeners();

        if(_gameManager != null)
        {
            _gameManager.onPointScored.RemoveListener(OnPointScored);
            _gameManager.onShotMiss.RemoveListener(OnShotMiss);
        }

        if(_bonusManager != null)
        {
            _bonusManager.bonusRemoved -= OnBonusRemoved;
        }
    }
}

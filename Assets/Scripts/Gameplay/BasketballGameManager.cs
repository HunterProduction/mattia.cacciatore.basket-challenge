using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BasketballGameManager : MonoBehaviourSingleton<BasketballGameManager>
{
    [Header("References")]
    [SerializeField] private BasketballHoop hoop;
    [SerializeField] private BasketballCourt court;
    [SerializeField] private BasketballCameraTarget cameraTarget;

    [Header("Game Data")]
    [SerializeField] private GameConfigData gameConfig;
    [SerializeField] private GameResultData gameResult;

    [Header("Time")]
    [SerializeField] private float shootTimeoutTime = 2.5f;
    [SerializeField] private float scoreNotificationTime = 1.5f;

    [Header("Events")]
    public UnityEvent onGameOver;

    private Dictionary<int, BasketballPlayer> _playersIdMap;
    private Dictionary<int, int> _playerBallScoresMap;
    private Dictionary<int, int> _ballToPlayerIdsMap;
    private Dictionary<string, ScoreBonus> _currentActiveBonuses;

    private float _gameTimeElapsed;

    #region Public Properties
    public GameConfigData GameConfigs => gameConfig;
    public BasketballPlayer[] Players => _playersIdMap.Values.ToArray();
    public float TimeElapsed => _gameTimeElapsed;
    public float TimeRemaining => Mathf.Max(0f, gameConfig.gameDuration - _gameTimeElapsed);
    #endregion

    #region Public Methods
    public int GetPlayerScore(BasketballPlayer player)
    {
        return _playerBallScoresMap[player.Ball.GetInstanceID()];
    }

    public int GetPlayerScore(int playerId)
    {
        return _playerBallScoresMap[_playersIdMap[playerId].Ball.GetInstanceID()];
    }

    public void AddBonus(ScoreBonus bonus, float expiresIn = 0f)
    {
        var success = _currentActiveBonuses.TryAdd(bonus.Id, bonus);
        if(success && expiresIn > 0f)
            StartCoroutine(RemoveBonusAfterTimeCoroutine(bonus, expiresIn));
    }

    public void RemoveBonus(ScoreBonus bonus)
    {
        _currentActiveBonuses.Remove(bonus.Id);
    }
    #endregion

    public override void Awake()
    {
        base.Awake();

        if(hoop == null)
        {
            hoop = FindObjectOfType<BasketballHoop>();
        }

        if(court == null)
        {
            court = FindObjectOfType<BasketballCourt>();
        }

        _currentActiveBonuses = new Dictionary<string, ScoreBonus>();
        _playersIdMap = new Dictionary<int, BasketballPlayer>();
        _playerBallScoresMap = new Dictionary<int, int>();
        _ballToPlayerIdsMap = new Dictionary<int, int>();

        var players = FindObjectsByType<BasketballPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            _playersIdMap.Add(player.GetInstanceID(), player);
            _playerBallScoresMap.Add(player.Ball.GetInstanceID(), 0);
            _ballToPlayerIdsMap.Add(player.Ball.GetInstanceID(), player.GetInstanceID());

            player.onBallShot.AddListener(() => StartCoroutine(ShotTimeoutCoroutine(player)));
        }

        hoop.onPointScored.AddListener(OnPointScored);
    }

    private void Update()
    {
        _gameTimeElapsed += Time.deltaTime;
        if(_gameTimeElapsed >= gameConfig.gameDuration)
        {
            Debug.Log($"[{GetType().Name}] Game Over!");

            onGameOver?.Invoke();
        }
    }

    private void OnPointScored(ScoredPointArgs scoredPointArgs)
    {
        // Compute base score value
        var score = scoredPointArgs.shotType switch
        {
            ShotType.PerfectShot => gameConfig.perfectShotScore,
            ShotType.RingShot => gameConfig.ringShotScore,
            ShotType.BackboardShot => gameConfig.backboardShotScore,
            _ => 0,
        };

        // Apply current active bonuses
        foreach (var bonus in _currentActiveBonuses.Values)
        {
            bonus.ApplyBonus(ref score);
        }

        // Update player score
        _playerBallScoresMap[scoredPointArgs.ballId] += score;

        var player = _playersIdMap[_ballToPlayerIdsMap[scoredPointArgs.ballId]];
        Debug.Log($"[{GetType().Name}] Player {player.name} scored {score} points");
        StartCoroutine(PointScoredCoroutine(player));
    }

    private void OnDestroy()
    {
        if (hoop != null)
            hoop.onPointScored.RemoveListener(OnPointScored);

        onGameOver.RemoveAllListeners();
    }

    #region Coroutines 
    private IEnumerator PointScoredCoroutine(BasketballPlayer player)
    {
        cameraTarget.enabled = false;

        yield return new WaitForSeconds(scoreNotificationTime);

        court.SetPlayerNextPosition(player);
        cameraTarget.enabled = true;
    }

    private IEnumerator ShotTimeoutCoroutine(BasketballPlayer player)
    {
        var ballId = player.Ball.GetInstanceID();
        var timeElapsed = 0f;
        var initialScore = _playerBallScoresMap[ballId];
        var scored = false;

        while (!scored && timeElapsed < shootTimeoutTime)
        {
            if (_playerBallScoresMap[ballId] != initialScore)
                scored = true;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (!scored)
            player.ResetPlayer(player.transform.position);
    }

    private IEnumerator RemoveBonusAfterTimeCoroutine(ScoreBonus bonus, float expiresIn)
    {
        yield return new WaitForSeconds(expiresIn);
        _currentActiveBonuses.Remove(bonus.Id);
    }
    #endregion
}

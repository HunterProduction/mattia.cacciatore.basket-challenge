using System;
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

    private Dictionary<BasketballPlayer, int> _playerScoresMap;
    private Dictionary<string, ShotScoreBonus> _currentActiveBonuses;

    private float _gameTimeElapsed;

    #region Public Properties
    public GameConfigData GameConfigs => gameConfig;
    public BasketballPlayer[] Players => _playerScoresMap.Keys.ToArray();
    public float TimeElapsed => _gameTimeElapsed;
    public float TimeRemaining => Mathf.Max(0f, gameConfig.gameDuration - _gameTimeElapsed);
    #endregion

    #region Public Methods
    public int GetPlayerScore(BasketballPlayer player)
    {
        return _playerScoresMap[player];
    }

    public void AddBonus(ShotScoreBonus bonus, float expiresIn = 0f)
    {
        var success = _currentActiveBonuses.TryAdd(bonus.Id, bonus);
        if(success && expiresIn > 0f)
            StartCoroutine(RemoveBonusAfterTimeCoroutine(bonus, expiresIn));
    }

    public void RemoveBonus(ShotScoreBonus bonus)
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

        _currentActiveBonuses = new Dictionary<string, ShotScoreBonus>();
        _playerScoresMap = new Dictionary<BasketballPlayer, int>();

        var players = FindObjectsByType<BasketballPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            _playerScoresMap.Add(player, 0);
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

            var scores = _playerScoresMap.Values.ToArray();
            Array.Sort(scores);

            // #TODO: #MattiaCacciatore Retrieve user's player and determin match result.

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
            if(bonus.IsAppliedTo(scoredPointArgs.shotType))
                bonus.ApplyBonus(ref score);
        }

        // Update player score
        _playerScoresMap[scoredPointArgs.player] += score;

        Debug.Log($"[{GetType().Name}] Player {scoredPointArgs.player.name} scored {score} points");
        StartCoroutine(PointScoredCoroutine(scoredPointArgs.player));
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
        var timeElapsed = 0f;
        var initialScore = _playerScoresMap[player];
        var scored = false;

        while (!scored && timeElapsed < shootTimeoutTime)
        {
            if (_playerScoresMap[player] != initialScore)
                scored = true;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (!scored)
            player.ResetPlayer(player.transform.position);
    }

    private IEnumerator RemoveBonusAfterTimeCoroutine(Bonus bonus, float expiresIn)
    {
        yield return new WaitForSeconds(expiresIn);
        _currentActiveBonuses.Remove(bonus.Id);
    }
    #endregion
}

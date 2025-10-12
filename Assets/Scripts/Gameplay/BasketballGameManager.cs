using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// #TODO: Consider making this a singleton 
public class BasketballGameManager : MonoBehaviourSingleton<BasketballGameManager>
{
    [Header("References")]
    [SerializeField] private BasketballHoop hoop;
    [SerializeField] private BasketballCourt court;

    [Header("Score")]
    [SerializeField] private int perfectShotScore = 5;
    [SerializeField] private int ringShotScore = 2;
    [SerializeField] private int backboardShotScore = 2;

    [Header("Time")]
    [SerializeField] private float shootTimeoutTime = 2.5f;
    [SerializeField] private float scoreNotificationTime = 1.5f;

    private Dictionary<int, BasketballPlayer> _playersIdMap;
    private Dictionary<int, int> _playerBallScoresMap;
    private Dictionary<int, int> _ballToPlayerIdsMap;

    public BasketballPlayer[] Players => _playersIdMap.Values.ToArray();

    public int GetPlayerScore(BasketballPlayer player)
    {
        return _playerBallScoresMap[player.Ball.GetInstanceID()];
    }

    public int GetPlayerScore(int playerId)
    {
        return _playerBallScoresMap[_playersIdMap[playerId].Ball.GetInstanceID()];
    }

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

    private void OnPointScored(ScoredPointArgs scoredPointArgs)
    {
        var score = scoredPointArgs.shotType switch
        {
            ShotType.PerfectShot => perfectShotScore,
            ShotType.RingShot => ringShotScore,
            ShotType.BackboardShot => backboardShotScore,
            _ => 0,
        };
        _playerBallScoresMap[scoredPointArgs.ballId] += score;

        var player = _playersIdMap[_ballToPlayerIdsMap[scoredPointArgs.ballId]];
        Debug.Log($"[{GetType().Name}] Player {player.name} scored {score} points");
        StartCoroutine(PointScoredCoroutine(player));
    }

    private IEnumerator PointScoredCoroutine(BasketballPlayer player)
    {
        yield return new WaitForSeconds(scoreNotificationTime);

        court.SetPlayerNextPosition(player);
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

    private void OnDestroy()
    {
        if (hoop != null)
            hoop.onPointScored.RemoveListener(OnPointScored);
    }
}

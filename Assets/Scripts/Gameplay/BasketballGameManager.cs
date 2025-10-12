using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// #TODO: Consider making this a singleton 
public class BasketballGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BasketballHoop hoop;

    [Header("Score")]
    [SerializeField] private int perfectShotScore = 5;
    [SerializeField] private int ringShotScore = 2;
    [SerializeField] private int backboardShotScore = 2;

    [Header("Time")]
    [SerializeField] private float shootTimeoutTime = 2.5f;
    [SerializeField] private float scoreNotificationTime = 1.5f;

    private Dictionary<int, BasketballPlayer> _playerIdsMap;
    private Dictionary<int, int> _playerBallScoresMap;
    private Dictionary<int, int> _ballToPlayerIdsMap;

    private void Awake()
    {
        if(hoop == null)
        {
            hoop = FindObjectOfType<BasketballHoop>();
        }

        _playerIdsMap = new Dictionary<int, BasketballPlayer>();
        _playerBallScoresMap = new Dictionary<int, int>();
        _ballToPlayerIdsMap = new Dictionary<int, int>();

        var players = FindObjectsByType<BasketballPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            _playerIdsMap.Add(player.GetInstanceID(), player);
            _playerBallScoresMap.Add(player.Ball.GetInstanceID(), 0);
            _ballToPlayerIdsMap.Add(player.Ball.GetInstanceID(), player.GetInstanceID());

            player.onBallShot.AddListener(() => StartCoroutine(ShotTimeoutCoroutine(player)));
        }

        hoop.onPointScored.AddListener(OnPointScored);
    }

    public int GetPlayerScore(BasketballPlayer player)
    {
        return _playerBallScoresMap[player.Ball.GetInstanceID()];
    }

    public int GetPlayerScore(int playerId)
    {
        return _playerBallScoresMap[_playerIdsMap[playerId].Ball.GetInstanceID()];
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

        var player = _playerIdsMap[_ballToPlayerIdsMap[scoredPointArgs.ballId]];
        Debug.Log($"[{GetType().Name}] Player {player.name} scored {score} points");
        StartCoroutine(PointScoredCoroutine(player));
    }

    private IEnumerator PointScoredCoroutine(BasketballPlayer player)
    {
        yield return new WaitForSeconds(scoreNotificationTime);

        player.ResetPlayer(player.transform.position);
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

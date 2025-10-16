using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballBonusManager : MonoBehaviourSingleton<BasketballBonusManager>
{
    private Dictionary<string, ShotScoreBonus> _currentActiveShotBonuses;
    private Dictionary<BasketballPlayer, Dictionary<string, PlayerScoreBonus>> _currentActivePlayerBonuses;

    public event Action<string> bonusRemoved;

    public override void Awake()
    {
        base.Awake();

        _currentActiveShotBonuses = new Dictionary<string, ShotScoreBonus>();
        _currentActivePlayerBonuses = new Dictionary<BasketballPlayer, Dictionary<string, PlayerScoreBonus>>();

        var players = FindObjectsByType<BasketballPlayer>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            _currentActivePlayerBonuses.Add(player, new Dictionary<string, PlayerScoreBonus>());
        }        
    }

    private void Start()
    {
        BasketballGameManager.Instance.onGameOver.AddListener(_ => StopAllCoroutines());
    }

    public void ApplyActiveBonusesTo(ref int score, BallEnteredArgs ballEnteredArgs)
    {
        foreach (var bonus in _currentActiveShotBonuses.Values)
        {
            if (bonus.IsAppliedTo(ballEnteredArgs.shotType))
                bonus.ApplyBonus(ref score);
        }

        foreach(var bonus in _currentActivePlayerBonuses[ballEnteredArgs.player].Values)
        {
            bonus.ApplyBonus(ref score);
        }
    }

    /**
     * #NOTE: #MattiaCacciatore This overload-based architecture is not the most scalable one if the project was meant to support
     * a variety of different bonus types. For the sake of this test, it is considered a good compromise.
     */

    #region AddBonus overloads
    public void AddBonus(ShotScoreBonus bonus)
    {
        Debug.Log($"[{GetType().Name}] Added bonus {bonus.Id} of type {bonus.GetType().Name}");
        var success = _currentActiveShotBonuses.TryAdd(bonus.Id, bonus);
        if (success && bonus.ExpiresIn > 0f)
            StartCoroutine(RemoveBonusAfterTimeCoroutine(bonus));
    }

    public void AddBonus(PlayerScoreBonus bonus)
    {
        Debug.Log($"[{GetType().Name}] Added bonus {bonus.Id} of type {bonus.GetType().Name}");
        var success = _currentActivePlayerBonuses[bonus.Player].TryAdd(bonus.Id, bonus);
        if (success && bonus.ExpiresIn > 0f)
            StartCoroutine(RemoveBonusAfterTimeCoroutine(bonus));
    }
    #endregion

    #region RemoveBonus overloads
    public void RemoveBonus(ShotScoreBonus bonus)
    {
        if (_currentActiveShotBonuses.Remove(bonus.Id))
        {
            Debug.Log($"[{GetType().Name}] Removed bonus {bonus.Id} of type {bonus.GetType().Name}");
            bonusRemoved?.Invoke(bonus.Id);
        }
    }

    public void RemoveBonus(PlayerScoreBonus bonus)
    {
        if (_currentActivePlayerBonuses[bonus.Player].Remove(bonus.Id))
        {
            Debug.Log($"[{GetType().Name}] Removed bonus {bonus.Id} of type {bonus.GetType().Name}");
            bonusRemoved?.Invoke(bonus.Id);
        }
    }
    #endregion

    private IEnumerator RemoveBonusAfterTimeCoroutine(Bonus bonus)
    {
        var wait = new WaitForSeconds(bonus.ExpiresIn);
        yield return wait;

        if(bonus is PlayerScoreBonus playerScoreBonus)
        {
            RemoveBonus(playerScoreBonus);
        }
        else if(bonus is ShotScoreBonus shotScoreBonus)
        {
            RemoveBonus(shotScoreBonus);
        }
    }
}

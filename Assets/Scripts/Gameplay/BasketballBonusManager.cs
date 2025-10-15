using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketballBonusManager : MonoBehaviourSingleton<BasketballBonusManager>
{
    private Dictionary<string, ShotScoreBonus> _currentActiveShotBonuses;
    private Dictionary<BasketballPlayer, Dictionary<string, PlayerScoreBonus>> _currentActivePlayerBonuses;

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
    public void AddBonus(ShotScoreBonus bonus, float expiresIn = 0f)
    {
        Debug.Log($"[{GetType().Name}] Added bonus {bonus.Id} of type {bonus.GetType().Name}");
        var success = _currentActiveShotBonuses.TryAdd(bonus.Id, bonus);
        if (success && expiresIn > 0f)
            StartCoroutine(RemoveBonusAfterTimeCoroutine(bonus, expiresIn));
    }

    public void AddBonus(PlayerScoreBonus bonus, float expiresIn = 0f)
    {
        Debug.Log($"[{GetType().Name}] Added bonus {bonus.Id} of type {bonus.GetType().Name}");
        var success = _currentActivePlayerBonuses[bonus.Player].TryAdd(bonus.Id, bonus);
        if (success && expiresIn > 0f)
            StartCoroutine(RemoveBonusAfterTimeCoroutine(bonus, expiresIn));
    }
    #endregion

    #region RemoveBonus overloads
    public void RemoveBonus(ShotScoreBonus bonus)
    {
        Debug.Log($"[{GetType().Name}] Removed bonus {bonus.Id} of type {bonus.GetType().Name}");
        _currentActiveShotBonuses.Remove(bonus.Id);
    }

    public void RemoveBonus(PlayerScoreBonus bonus)
    {
        Debug.Log($"[{GetType().Name}] Removed bonus {bonus.Id} of type {bonus.GetType().Name}");
        _currentActivePlayerBonuses[bonus.Player].Remove(bonus.Id);
    }
    #endregion

    private IEnumerator RemoveBonusAfterTimeCoroutine(Bonus bonus, float expiresIn)
    {
        var wait = new WaitForSeconds(expiresIn);
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

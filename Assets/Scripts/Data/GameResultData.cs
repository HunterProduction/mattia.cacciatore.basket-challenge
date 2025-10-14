using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerStats
{
    public string Id { get; private set; }

    public int score;

    public PlayerStats(string id)
    {
        Id = id;
    }
}

[CreateAssetMenu(fileName = "GameResultData", menuName = "Basketball/Game Result Data", order = 1)]
public class GameResultData : ScriptableObject
{
    public MatchResult matchResult;

    private Dictionary<string, PlayerStats> _playerStats;

    public void Initialize(BasketballPlayer[] players)
    {
        _playerStats = new Dictionary<string, PlayerStats>();
        foreach (var player in players)
            _playerStats.Add(player.Id, new PlayerStats(player.Id));

        matchResult = MatchResult.Ongoing;
    }

    public PlayerStats GetPlayerStats(BasketballPlayer player) => _playerStats[player.Id];
    public PlayerStats GetPlayerStats(string playerId) => _playerStats[playerId];
    public PlayerStats[] GetAllPlayerStats(bool sorted = false)
    {
        if(sorted)
            return _playerStats.Values.OrderByDescending(stats => stats.score).ToArray();
        return _playerStats.Values.ToArray();
    }
}

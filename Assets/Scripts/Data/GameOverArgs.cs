public struct GameOverArgs
{
    public MatchResult matchResult;
    public BasketballPlayer winner;

    public GameOverArgs(MatchResult matchResult, BasketballPlayer winner)
    {
        this.matchResult = matchResult;
        this.winner = winner;
    }
}

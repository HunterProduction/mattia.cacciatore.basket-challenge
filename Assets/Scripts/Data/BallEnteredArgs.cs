public struct BallEnteredArgs
{
    public ShotType shotType;
    public BasketballPlayer player;

    public BallEnteredArgs(ShotType shotType, BasketballPlayer player)
    {
        this.shotType = shotType;
        this.player = player;
    }
}

public struct PointScoredArgs
{
    public int pointScored;
    public BallEnteredArgs shotData;

    public PointScoredArgs(int pointScored, BallEnteredArgs shotData)
    {
        this.pointScored = pointScored;
        this.shotData = shotData;
    }
}

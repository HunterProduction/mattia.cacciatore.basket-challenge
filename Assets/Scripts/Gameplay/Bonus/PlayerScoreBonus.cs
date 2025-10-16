using System;

[Serializable]
public class PlayerScoreBonus : Bonus
{
    private BasketballPlayer _player;
    public BasketballPlayer Player => _player;

    public PlayerScoreBonus(BasketballPlayer player, float bonusValue, ApplyType bonusType, int expiresIn, string id = "") : base(bonusValue, bonusType, expiresIn, id)
    {
        this._player = player;
    }
}

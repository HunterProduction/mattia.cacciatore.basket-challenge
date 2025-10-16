using System;
using UnityEngine;

public class FireballBonusDispatcher : MonoBehaviour
{
    //#TODO
    [SerializeField] private GameConfigData gameConfig;
    [SerializeField] private BasketballPlayer player;

    private BasketballGameManager _gameManager;

    private void Awake()
    {
        _gameManager = BasketballGameManager.Instance;

        _gameManager.onPointScored.AddListener(OnPointScored);
    }

    private void OnPointScored(PointScoredArgs pointScoredArgs)
    {
        throw new NotImplementedException();
    }
}

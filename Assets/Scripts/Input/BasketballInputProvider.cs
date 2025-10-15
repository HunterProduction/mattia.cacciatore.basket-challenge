using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public abstract class BasketballInputProvider : MonoBehaviour
{
    [SerializeField] protected BasketballPlayer player;

    [Header("Events")]
    public UnityEvent onInputStarted;
    public UnityEvent<float> onInputPerformed;

    protected float _currentValue;
    public float CurrentValue => Mathf.Sqrt(_currentValue);

    private BasketballGameManager _gameManager;

    protected virtual void Start()
    {
        _gameManager = BasketballGameManager.Instance;
        enabled = false;
        _gameManager.onGameStarted.AddListener(() => enabled = true);
        _gameManager.onGameOver.AddListener(_ => enabled = false);
    }

    protected virtual void OnEnable()
    {
        _currentValue = player.MinShotVelocity.sqrMagnitude;
    }

    protected virtual void SendInput()
    {
        onInputPerformed.Invoke(CurrentValue);

        _currentValue = player.MinShotVelocity.sqrMagnitude;
    }

    protected virtual void OnDestroy()
    {
        onInputPerformed.RemoveAllListeners();
        onInputStarted.RemoveAllListeners();

        if(_gameManager != null)
        {
            _gameManager.onGameStarted.RemoveListener(() => enabled = true);
            _gameManager.onGameOver.RemoveListener(_ => enabled = false);
        }
    }
}

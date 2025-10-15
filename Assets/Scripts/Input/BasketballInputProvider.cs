using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public abstract class BasketballInputProvider : MonoBehaviour
{
    [SerializeField] protected BasketballPlayer player;

    [Header("Events")]
    public UnityEvent onInputStarted;
    public UnityEvent<float> onInputPerformed;

    protected float _currentValueSquared;
    public float CurrentValue => Mathf.Sqrt(_currentValueSquared);

    private BasketballGameManager _gameManager;

    protected virtual void Awake()
    {
        _gameManager = BasketballGameManager.Instance;
        enabled = false;
        Debug.Log($"[{GetType().Name}] {gameObject.name} started");
        _gameManager.onGameStarted.AddListener(() => enabled = true);
        _gameManager.onGameOver.AddListener(_ => enabled = false);
    }

    protected virtual void OnEnable()
    {
        _currentValueSquared = player.MinShotVelocity.sqrMagnitude;
    }

    protected virtual void SendInput()
    {
        onInputPerformed.Invoke(CurrentValue);

        _currentValueSquared = player.MinShotVelocity.sqrMagnitude;
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

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// #TODO: Rethink class naming and architecture.
public class PlayerInputProvider : MonoBehaviour
{
    [SerializeField] private BasketballPlayer player;
    [SerializeField] private float maxTimeFrame = 2f;

    [Header("Events")]
    public UnityEvent onInputStarted;
    public UnityEvent<float> onInputPerformed;

    private float _inputValue;
    private float _timeElapsed;
    private bool _pressed;

    public float CurrentValue => Mathf.Sqrt(_inputValue);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !_pressed)
        {
            _pressed = true;
            StartCoroutine(ReadInputCoroutine());
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            _pressed = false;
        }
    }


    private IEnumerator ReadInputCoroutine()
    {
        onInputStarted.Invoke();
        /*
         *  #NOTE: Where it's mathematically possible, magnitudes are kept squared to avoid
         *  repeated square root computations.
         */
        var min = player.MinShotVelocity.sqrMagnitude;
        var max = player.MaxShotVelocity.sqrMagnitude;

        _inputValue = player.MinShotVelocity.sqrMagnitude;
        _timeElapsed = 0f;
        while (_pressed && _timeElapsed <= maxTimeFrame)
        {
            var speed = (max - min) / maxTimeFrame;
            _inputValue = Mathf.Clamp(_inputValue + speed * Time.deltaTime, min, max);
            Debug.Log($"[{GetType().Name}] Input value: {_inputValue}");

            _timeElapsed += Time.deltaTime;
            yield return null;
        }

        SendInput();
    }

    private void SendInput()
    {
        onInputPerformed.Invoke(CurrentValue);
        _pressed = false;
    }

    private void OnDestroy()
    {
        onInputPerformed.RemoveAllListeners();
        onInputStarted.RemoveAllListeners();
    }
}

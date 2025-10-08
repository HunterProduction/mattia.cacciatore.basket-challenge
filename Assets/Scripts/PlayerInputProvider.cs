using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// #TODO: Rethink class naming and architecture.
public class PlayerInputProvider : MonoBehaviour
{
    [Range(0f, 15f)]
    public float minInputVelocity, maxInputVelocity;
    [SerializeField] private float maxTimeFrame = 2f;

    [Header("UI")]
    [SerializeField] private Slider slider;

    public UnityEvent onInputStarted;
    public UnityEvent<float> onInputPerformed;

    private float _inputValue;
    private float _timeElapsed;
    private bool _pressed;

    public float CurrentValue => _inputValue;

    private void Awake()
    {
        slider.minValue = minInputVelocity;
        slider.maxValue = maxInputVelocity;
    }

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
        _timeElapsed = 0f;
        _inputValue = minInputVelocity;
        onInputStarted.Invoke();
        while (_pressed && _timeElapsed <= maxTimeFrame)
        {
            var speed = (maxInputVelocity - minInputVelocity) / maxTimeFrame;
            _inputValue = Mathf.Clamp(_inputValue + speed * Time.deltaTime, minInputVelocity, maxInputVelocity);
            Debug.Log($"[{GetType().Name}] Input value: {_inputValue}");

            _timeElapsed += Time.deltaTime;
            yield return null;
        }

        SendInput();
    }

    private void SendInput()
    {
        onInputPerformed.Invoke(_inputValue);
        _pressed = false;
    }

    private void OnDestroy()
    {
        onInputPerformed.RemoveAllListeners();
        onInputStarted.RemoveAllListeners();
    }
}

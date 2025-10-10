using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// #TODO: Rethink class naming.
public class PlayerInputProvider : MonoBehaviour
{
    [SerializeField] private BasketballPlayer player;
    [SerializeField] private float maxTimeFrame = 2f;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pointerAction;
    [SerializeField] private InputActionReference pressAction;

    [Header("Events")]
    public UnityEvent onInputStarted;
    public UnityEvent<float> onInputPerformed;

    private float _inputValue;
    private float _timeElapsed;
    private bool _pressed;

    public float CurrentValue => Mathf.Sqrt(_inputValue);


    private void OnEnable()
    {
        pressAction.action.performed += OnPressActionPerformed;
        pressAction.action.canceled += OnPressActionCanceled;
    }

    private void Update()
    {
        if (!_pressed)
            return;

        /*
         *  #NOTE: Where it's mathematically possible, magnitudes are kept squared to avoid
         *  repeated square root computations.
         */

        var min = player.MinShotVelocity.sqrMagnitude;
        var max = player.MaxShotVelocity.sqrMagnitude;

        var normalizedPointerDeltaY = Mathf.Clamp01(pointerAction.action.ReadValue<Vector2>().y / Screen.height);
        _inputValue = Mathf.Clamp(
                _inputValue + normalizedPointerDeltaY * (max - min),
                min,
                max);

        Debug.Log($"[{GetType().Name}] Input value: {_inputValue}");

        _timeElapsed += Time.deltaTime;

        if(_timeElapsed > maxTimeFrame)
            SendInput();
    }

    private void OnPressActionPerformed(InputAction.CallbackContext context)
    {
        _timeElapsed = 0f;
        _inputValue = player.MinShotVelocity.sqrMagnitude;

        _pressed = true;
        onInputStarted.Invoke();
    }

    private void OnPressActionCanceled(InputAction.CallbackContext context)
    {
        if (_pressed)
        {
            SendInput();
        }
    }

    private void SendInput()
    {
        onInputPerformed.Invoke(CurrentValue);

        _inputValue = player.MinShotVelocity.sqrMagnitude;
        _pressed = false;
    }

    private void OnDisable()
    {
        pressAction.action.performed -= OnPressActionPerformed;
        pressAction.action.canceled -= OnPressActionCanceled;
    }

    private void OnDestroy()
    {
        onInputPerformed.RemoveAllListeners();
        onInputStarted.RemoveAllListeners();
    }
}

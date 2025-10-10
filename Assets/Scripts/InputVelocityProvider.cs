using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// #TODO: Rethink class naming.
public class InputVelocityProvider : MonoBehaviour
{
    [SerializeField] private BasketballPlayer player;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pressAction;
    [SerializeField] private InputActionReference dragAction;

    [Header("Parameters")]
    [SerializeField] private float maxTimeFrame = 2f;
    [Range(0f, 2f)]
    [SerializeField] private float accelerationBoost = 0.2f;

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

        var deltaPointerPixel = dragAction.action.ReadValue<Vector2>();

        // Normalized delta in [0, 1]
        var deltaY = Mathf.Clamp01(deltaPointerPixel.y / Screen.height);

        // Acceleration term
        var gain = 1f + accelerationBoost * (1f - Mathf.Exp(-deltaY / Time.deltaTime));

        var deltaInput = deltaY * (max - min) * gain;
        _inputValue = Mathf.Clamp(_inputValue + deltaInput, min, max);

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

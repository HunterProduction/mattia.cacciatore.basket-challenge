using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FollowPointer : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference pressAction;
    [SerializeField] private InputActionReference pointerPositionAction;

    [Header("Parameters")]
    [SerializeField] private bool activateOnHold = true;
    [SerializeField] private float distanceFromCamera = 1f;

    private bool _isActive, _isUi;
    private Camera _camera;

    private void OnEnable()
    {
        _isUi = TryGetComponent<RectTransform>(out _);
        pressAction.action.performed += OnPressActionPerformed;
        pressAction.action.canceled += OnPressActionCanceled;
        _camera = Camera.main;
    }    

    private void OnDisable()
    {
        pressAction.action.performed -= OnPressActionPerformed;
        pressAction.action.canceled -= OnPressActionCanceled;
    }

    private void Update()
    {
        if (!_isActive)
            return;

        var pointerPos = pointerPositionAction.action.ReadValue<Vector2>();
        Vector3 pointerWorldPos = _camera.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, distanceFromCamera));
        transform.position = _isUi ? pointerPos : pointerWorldPos;
    }

    private void OnPressActionPerformed(InputAction.CallbackContext context)
    {
        if(activateOnHold)
            _isActive = true;
    }

    private void OnPressActionCanceled(InputAction.CallbackContext context)
    {
        if (activateOnHold)
            _isActive = false;
    }
}

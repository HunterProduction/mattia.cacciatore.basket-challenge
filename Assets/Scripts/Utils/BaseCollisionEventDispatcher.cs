using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public abstract class BaseCollisionEventDispatcher<TArgument> : MonoBehaviour
{
    enum SendEventMode
    {
        UnityEvent,
        CSharpAction,
        Both
    }

    [SerializeField] protected LayerMask layerMask;
    [SerializeField] private SendEventMode sendEventMode;
    [Range(0f, 3f)]
    [SerializeField] private float sendEventCooldownTime;

    [Header("Unity Events")]
    public UnityEvent<TArgument> onEnter;
    public UnityEvent<TArgument> onStay;
    public UnityEvent<TArgument> onExit;

    public event Action<TArgument> entered, stay, exited;

    protected Collider _collider;
    private bool _enterFired, _exitFired;
    private float _enterCooldown, _exitCooldown;

    protected virtual void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    protected abstract int GetCollidedObjectLayer(TArgument argument);

    protected void OnEnter(TArgument other)
    {
        if (!enabled)
            return;

        if (layerMask.ContainsLayer(GetCollidedObjectLayer(other)) && !_enterFired)
        {
            _enterFired = true;
            switch (sendEventMode)
            {
                case SendEventMode.UnityEvent:
                    onEnter?.Invoke(other);
                    break;
                case SendEventMode.CSharpAction:
                    entered?.Invoke(other);
                    break;
                case SendEventMode.Both:
                    entered?.Invoke(other);
                    onEnter?.Invoke(other);
                    break;
                default:
                    break;
            }
        }
    }

    protected void OnStay(TArgument other)
    {
        if (!enabled)
            return;

        if (layerMask.ContainsLayer(GetCollidedObjectLayer(other)))
        {
            switch (sendEventMode)
            {
                case SendEventMode.UnityEvent:
                    onStay?.Invoke(other);
                    break;
                case SendEventMode.CSharpAction:
                    stay?.Invoke(other);
                    break;
                case SendEventMode.Both:
                    stay?.Invoke(other);
                    stay?.Invoke(other);
                    break;
                default:
                    break;
            }
        }
    }

    protected void OnExit(TArgument other)
    {
        if (!enabled)
            return;

        if (layerMask.ContainsLayer(GetCollidedObjectLayer(other)) && !_exitFired)
        {
            _exitFired = true;
            switch (sendEventMode)
            {
                case SendEventMode.UnityEvent:
                    onExit?.Invoke(other);
                    break;
                case SendEventMode.CSharpAction:
                    exited?.Invoke(other);
                    break;
                case SendEventMode.Both:
                    exited?.Invoke(other);
                    exited?.Invoke(other);
                    break;
                default:
                    break;
            }
        }
    }

    protected virtual void Update()
    {
        UpdateCooldown(ref _enterFired, ref _enterCooldown);
        UpdateCooldown(ref _exitFired, ref _exitCooldown);
    }

    private void UpdateCooldown(ref bool fired, ref float cooldown)
    {
        if (fired)
        {
            if (cooldown < sendEventCooldownTime)
                cooldown += Time.deltaTime;
            else
            {
                fired = false;
                cooldown = 0;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        onEnter.RemoveAllListeners();
        onStay.RemoveAllListeners();
        onExit.RemoveAllListeners();
    }
}

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerEventDispatcher : BaseCollisionEventDispatcher<Collider>
{
    protected override void Awake()
    {
        base.Awake();
        _collider.isTrigger = true;
    }

    protected override int GetCollidedObjectLayer(Collider argument)
    {
        return argument.gameObject.layer;
    }

    private void OnTriggerEnter(Collider other) => OnEnter(other);
    private void OnTriggerStay(Collider other) => OnStay(other);
    private void OnTriggerExit(Collider other) => OnExit(other);
}

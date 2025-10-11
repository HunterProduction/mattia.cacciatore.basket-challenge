using UnityEngine;

public class CollisionEventDispatcher : BaseCollisionEventDispatcher<Collision>
{
    protected override int GetCollidedObjectLayer(Collision argument)
    {
        return argument.gameObject.layer;
    }

    private void OnCollisionEnter(Collision collision) => OnEnter(collision);
    private void OnCollisionStay(Collision collision) => OnStay(collision);
    private void OnCollisionExit(Collision collision) => OnExit(collision);
}

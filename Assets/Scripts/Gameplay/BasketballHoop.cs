using UnityEngine;
using UnityEngine.Events;

public class BasketballHoop : MonoBehaviour
{
    [Header("Transform references")]
    [SerializeField] private Transform baseTransform;
    [SerializeField] private Transform perfectTarget;
    [SerializeField] private Transform backboardTargetCenter, backboardTargetLeft, backboardTargetRight;

    public Transform Base => baseTransform;
    public Transform PerfectTarget => perfectTarget;
    public Transform BackboardTargetCenter => backboardTargetCenter;
    public Transform BackboardTargetLeft => backboardTargetLeft;
    public Transform BackboardTargetRight => backboardTargetRight;

    [Header("Collision Detection")]
    [SerializeField] private TriggerEventDispatcher hoopTrigger;
    [SerializeField] private CollisionEventDispatcher backboardCollision, ringCollision;

    [Header("Events")]
    public UnityEvent onPointScored;

    private void OnEnable()
    {
        if(hoopTrigger == null)
        {
            hoopTrigger = GetComponentInChildren<TriggerEventDispatcher>();
        }

        hoopTrigger.entered += OnBallEntered;
        backboardCollision.entered += OnBallTouchedBackboard;
        ringCollision.entered += OnBallTouchedRing;
    }

    private void OnBallEntered(Collider ballCollider)
    {
        Debug.Log($"[{GetType().Name}] {ballCollider.gameObject.name} Entered!");
    }

    private void OnBallTouchedRing(Collision ringCollision)
    {
        Debug.Log($"[{GetType().Name}] {ringCollision.gameObject.name} touched Ring.");
    }

    private void OnBallTouchedBackboard(Collision barkboardCollision)
    {
        Debug.Log($"[{GetType().Name}] {barkboardCollision.gameObject.name} touched Backboard.");
    }

    private void OnDisable()
    {
        if (hoopTrigger != null)
        {
            hoopTrigger.entered -= OnBallEntered;
        }
    }
}

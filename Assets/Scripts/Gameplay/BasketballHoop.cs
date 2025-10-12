using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ShotType
{
    PerfectShot,
    RingShot,
    BackboardShot,
    Miss
}

public struct ScoredPointArgs
{
    public ShotType shotType;
    public int ballId;

    public ScoredPointArgs(ShotType shotType, int ballId)
    {
        this.shotType = shotType;
        this.ballId = ballId;
    }
}

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
    public UnityEvent<ScoredPointArgs> onPointScored;

    private Dictionary<int, ShotType> _ballShotRegister;

    private void Awake()
    {
        _ballShotRegister = new Dictionary<int, ShotType>();
        foreach (var player in BasketballGameManager.Instance.Players)
        {
            _ballShotRegister.Add(player.Ball.GetInstanceID(), ShotType.Miss);
        }
    }

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
        var ballId = ballCollider.attachedRigidbody.GetInstanceID();

        var shotType = _ballShotRegister[ballId];
        if (shotType != ShotType.RingShot && shotType != ShotType.BackboardShot)
            shotType = ShotType.PerfectShot;

        Debug.Log($"[{GetType().Name}] {ballCollider.gameObject.name}({ballId}) {shotType} scored!");
        onPointScored?.Invoke(new ScoredPointArgs(shotType, ballId));

        _ballShotRegister[ballId] = ShotType.Miss;
    }

    private void OnBallTouchedRing(Collision ringCollision)
    {
        var ballId = ringCollision.rigidbody.GetInstanceID();

        Debug.Log($"[{GetType().Name}] {ringCollision.gameObject.name}({ballId}) touched Ring.");

        if (_ballShotRegister[ballId] != ShotType.BackboardShot)
            _ballShotRegister[ballId] = ShotType.RingShot;
    }
     
    private void OnBallTouchedBackboard(Collision barkboardCollision)
    {
        var ballId = barkboardCollision.rigidbody.GetInstanceID();

        Debug.Log($"[{GetType().Name}] {barkboardCollision.gameObject.name}({ballId}) touched Backboard.");

        if(_ballShotRegister[ballId] != ShotType.RingShot)
            _ballShotRegister[ballId] = ShotType.BackboardShot;
    }

    private void OnDisable()
    {
        if (hoopTrigger != null)
        {
            hoopTrigger.entered -= OnBallEntered;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = new Color(0, 1, 0, .4f);
        var radius = 0.1f;

        if (perfectTarget != null)
            Handles.DrawSolidDisc(perfectTarget.position, transform.up, radius);
        if(backboardTargetCenter != null)
            Handles.DrawSolidDisc(backboardTargetCenter.position, transform.forward, radius);
        if (backboardTargetLeft != null)
            Handles.DrawSolidDisc(backboardTargetLeft.position, transform.forward, radius);
        if (backboardTargetRight != null)
            Handles.DrawSolidDisc(backboardTargetRight.position, transform.forward, radius);
    }
#endif
}

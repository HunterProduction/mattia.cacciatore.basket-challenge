using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public struct ScoredPointArgs
{
    public ShotType shotType;
    public BasketballPlayer player;

    public ScoredPointArgs(ShotType shotType, BasketballPlayer player)
    {
        this.shotType = shotType;
        this.player = player;
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

    [Header("Score bonus")]
    [SerializeField] private RandomBonusEventsDispatcher backboardBonusEventsDispatcher;
    [SerializeField] private float bonusTimeWindow = 6f;

    [Header("Events")]
    public UnityEvent<ScoredPointArgs> onPointScored;
    public UnityEvent<ShotScoreBonus> onBackboardBonusStarted, onBackboardBonusEnded;

    private Dictionary<BasketballBall, ShotType> _ballShotRegister;

    private void Start()
    {
        _ballShotRegister = new Dictionary<BasketballBall, ShotType>();
        foreach (var player in BasketballGameManager.Instance.Players)
        {
            _ballShotRegister.Add(player.Ball, ShotType.Miss);
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

        if (backboardBonusEventsDispatcher == null)
        {
            backboardBonusEventsDispatcher = GetComponentInChildren<RandomBonusEventsDispatcher>();
        }
        backboardBonusEventsDispatcher.bonusEventTriggered += OnBackboardBonusTriggered;
    }

    private void OnBallEntered(Collider ballCollider)
    {
        if (!ballCollider.gameObject.TryGetComponent<BasketballBall>(out var ball))
        {
            Debug.LogError($"[{GetType().Name}] {ballCollider.gameObject.name} has no BasketballBall component.", ringCollision.gameObject);
            return;
        }

        var shotType = _ballShotRegister[ball];
        if (shotType != ShotType.RingShot && shotType != ShotType.BackboardShot)
            shotType = ShotType.PerfectShot;

        Debug.Log($"[{GetType().Name}] {ballCollider.gameObject.name} {shotType} scored!");
        onPointScored?.Invoke(new ScoredPointArgs(shotType, ball.Owner));

        _ballShotRegister[ball] = ShotType.Miss;
    }

    private void OnBallTouchedRing(Collision ringCollision)
    {
        if(!ringCollision.gameObject.TryGetComponent<BasketballBall>(out var ball))
        {
            Debug.LogError($"[{GetType().Name}] {ringCollision.gameObject.name} has no BasketballBall component.", ringCollision.gameObject);
            return;
        }

        Debug.Log($"[{GetType().Name}] {ringCollision.gameObject.name} touched Ring.");

        if (_ballShotRegister[ball] != ShotType.BackboardShot)
            _ballShotRegister[ball] = ShotType.RingShot;
    }
     
    private void OnBallTouchedBackboard(Collision barkboardCollision)
    {
        if (!barkboardCollision.gameObject.TryGetComponent<BasketballBall>(out var ball))
        {
            Debug.LogError($"[{GetType().Name}] {barkboardCollision.gameObject.name} has no BasketballBall component.", ringCollision.gameObject);
            return;
        }

        Debug.Log($"[{GetType().Name}] {barkboardCollision.gameObject.name} touched Backboard.");

        if(_ballShotRegister[ball] != ShotType.RingShot)
           _ballShotRegister[ball] = ShotType.BackboardShot;
    }

    private void OnBackboardBonusTriggered(ShotScoreBonus bonus)
    {
        BasketballGameManager.Instance.AddBonus(bonus, bonusTimeWindow);
        StartCoroutine(BonusEventTimeWindowCoroutine(bonus));
    }

    private void OnDisable()
    {
        if (hoopTrigger != null)
            hoopTrigger.entered -= OnBallEntered;

        if (backboardBonusEventsDispatcher != null)
            backboardBonusEventsDispatcher.bonusEventTriggered -= OnBackboardBonusTriggered;
    }

    private void OnDestroy()
    {
        onPointScored.RemoveAllListeners();
    }

    private IEnumerator BonusEventTimeWindowCoroutine(ShotScoreBonus bonus)
    {
        backboardBonusEventsDispatcher.enabled = false;
        Debug.Log($"[{GetType().Name}] Bonus {bonus.Id} activated!");
        onBackboardBonusStarted?.Invoke(bonus);

        yield return new WaitForSeconds(bonusTimeWindow);

        Debug.Log($"[{GetType().Name}] Bonus {bonus.Id} deactivated!");
        onBackboardBonusEnded?.Invoke(bonus);
        backboardBonusEventsDispatcher.enabled = true;
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

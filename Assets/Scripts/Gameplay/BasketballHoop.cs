using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public UnityEvent<BallEnteredArgs> onBallEntered;
    public UnityEvent<ShotScoreBonus> onBackboardBonusStarted, onBackboardBonusEnded;
    [SerializeField] private float resetBallStateAfter = .5f;

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
        if (shotType == ShotType.Miss)
            shotType = ShotType.PerfectShot;

        Debug.Log($"[{GetType().Name}] {ballCollider.gameObject.name} {shotType} scored!");
        onBallEntered?.Invoke(new BallEnteredArgs(shotType, ball.Owner));

        // #NOTE: #MattiaCacciatore Not the most elegant solution to reset ball state after scoring in this architecture, but for the scope
        // of a non-production ready test it should be convenient.
        StartCoroutine(ResetBallCoroutine(ball));
    }

    private void OnBallTouchedRing(Collision ringCollision)
    {
        if(!ringCollision.gameObject.TryGetComponent<BasketballBall>(out var ball))
        {
            Debug.LogError($"[{GetType().Name}] {ringCollision.gameObject.name} has no BasketballBall component.", ringCollision.gameObject);
            return;
        }

        Debug.Log($"[{GetType().Name}] {ringCollision.gameObject.name} touched Ring.");

        if (_ballShotRegister[ball] == ShotType.Miss)
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

        if(_ballShotRegister[ball] == ShotType.Miss)
           _ballShotRegister[ball] = ShotType.BackboardShot;
    }

    private void OnBackboardBonusTriggered(ShotScoreBonus bonus)
    {
        BasketballGameManager.Instance.AddBonus(bonus, bonusTimeWindow);
        StartCoroutine(BonusEventTimeWindowCoroutine(bonus));
    }

    private IEnumerator ResetBallCoroutine(BasketballBall ball)
    {
        var wait = new WaitForSeconds(resetBallStateAfter);
        yield return wait;
        _ballShotRegister[ball] = ShotType.Miss;
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
        onBallEntered.RemoveAllListeners();
    }

    private IEnumerator BonusEventTimeWindowCoroutine(ShotScoreBonus bonus)
    {
        backboardBonusEventsDispatcher.enabled = false;
        Debug.Log($"[{GetType().Name}] Bonus {bonus.Id} activated!");
        onBackboardBonusStarted?.Invoke(bonus);

        var wait = new WaitForSeconds(bonusTimeWindow);
        yield return wait;

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

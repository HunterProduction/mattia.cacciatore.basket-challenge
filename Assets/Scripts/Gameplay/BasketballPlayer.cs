using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BasketballPlayer : MonoBehaviour
{
    public enum ShotAimMode
    {
        Perfect,
        Backboard
    }

    [SerializeField] private string id;
    public string Id => id;
    /**
     * #NOTE: #MattiaCacciatore This is a just simple placeholder way to fake the identification of the local player. 
     */
    [SerializeField] private bool isUser;
    public bool IsUser => isUser;

    [Header("References")]
    [SerializeField] private BasketballBall ball;
    public BasketballBall Ball => ball;

    [SerializeField] private BasketballCourt court;
    [SerializeField] private BasketballInputProvider inputProvider;
    [SerializeField] private Transform endGameCameraTarget;
    public Transform EndGameCameraTarget => endGameCameraTarget;

    [Header("Parameters")]
    [Range(-90f, 0f)]
    [SerializeField] private float perfectShotTangentAngle = -55f;
    [Range(-90f, 0f)]
    [SerializeField] private float backboardShotTangentAngle = -20f;
    [Range(.5f, 1f)]
    [SerializeField] private float minShotFactor = .9f;
    [Range(1f, 1.2f)]
    [SerializeField] private float maxShotFactor = 1.05f;

    [Header("Events")]
    public UnityEvent onBallShot;
    public UnityEvent onPositionReset;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private ShotAimMode previewShotType = ShotAimMode.Perfect;
    [Range(0.1f, 1f)]
    [SerializeField] private float timeDilationFactor = 1f;

    private Vector3 _perfectShotOptimalVelocity;
    public Vector3 PerfectShotOptimalVelocity => _perfectShotOptimalVelocity;

    private Vector3 _backboardShotOptimalVelocity;
    public Vector3 BackboardShotOptimalVelocity => _backboardShotOptimalVelocity;

    private Vector3 _minShotVelocity;
    public Vector3 MinShotVelocity => _minShotVelocity;

    private Vector3 _maxShotVelocity;
    public Vector3 MaxShotVelocity => _maxShotVelocity;


    private bool _computeVelocities = true;
    private float _gravityMagnitude;
    private MultiInterpolator<Vector3> _interpolator;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            id = "Player_" + GetInstanceID();
        }

        // #TODO: Find a smarter way to generalize this repeated check pattern.
        if (ball == null)
        {
            ball = GetComponentInChildren<BasketballBall>();
            if(ball == null)
            {
                throw new UnassignedReferenceException("Unable to retrieve reference");
            }
        }

        if(court == null)
        {
            court = FindObjectOfType<BasketballCourt>();
            if(court == null)
            {
                throw new UnassignedReferenceException("Unable to retrieve reference");
            }
        }

        ball.Owner = this;
        ball.Reset();

        // Caching gravity magnitude to spare square root computing
        _gravityMagnitude = Physics.gravity.magnitude;       
        
        _interpolator = new MultiInterpolator<Vector3>(new List<KeyValuePair<float, Vector3>> 
        {
            new(0f, Vector3.zero),
            new(1f, Vector3.zero),
        }, Vector3.Lerp);
    }

    private void OnEnable()
    {
        inputProvider.onInputPerformed.AddListener(OnInputReceived);
    }

    private void OnDisable()
    {
        inputProvider.onInputPerformed.AddListener(OnInputReceived);
    }

    private void Update()
    {
        if(_computeVelocities)
        {
            UpdateShotOptimalVelocities();
        }
    }

    public void OnInputReceived(float inputVelocity)
    {
        if (debug)
            Time.timeScale = timeDilationFactor;

        inputProvider.enabled = false;

        _interpolator.SetPairs(new List<KeyValuePair<float, Vector3>>
        {
            new(_minShotVelocity.sqrMagnitude, _minShotVelocity),
            new(_perfectShotOptimalVelocity.sqrMagnitude, _perfectShotOptimalVelocity),
            new(_backboardShotOptimalVelocity.sqrMagnitude, _backboardShotOptimalVelocity),
            new(_maxShotVelocity.sqrMagnitude, _maxShotVelocity),
        });

        var velocity = _interpolator.Evaluate(inputVelocity * inputVelocity);
        Debug.Log($"[{GetType().Name}] Perfect Shot opt. = {_perfectShotOptimalVelocity.magnitude}, Backboard Shot opt. = {_backboardShotOptimalVelocity.magnitude}\n Velocity = {velocity.magnitude}");

        ShootBall(velocity);
    }

    private void ShootBall(Vector3 initialVelocity)
    {
        _computeVelocities = false;
        ball.Shoot(initialVelocity);
        onBallShot?.Invoke();
    }

    public void ResetPlayer(Vector3 newShootPosition, bool inputEnabled = true)
    {
        if (debug)
            Time.timeScale = 1;

        transform.position = newShootPosition;
        ball.Reset();
        inputProvider.enabled = inputEnabled;
        _computeVelocities = true;
        onPositionReset?.Invoke();
    }

    private void UpdateShotOptimalVelocities()
    {
        var start = ball.transform.position;

        var backboardShotTarget = court.GetHoopTarget(transform.position, ShotAimMode.Backboard);
        var perfectShotTarget = court.GetHoopTarget(transform.position, ShotAimMode.Perfect);

        TryGetPerfectVelocity(start, perfectShotTarget,
            perfectShotTangentAngle,
            out _perfectShotOptimalVelocity);

        TryGetPerfectVelocity(start, backboardShotTarget,
            backboardShotTangentAngle,
            out _backboardShotOptimalVelocity);

        _minShotVelocity = (_perfectShotOptimalVelocity.magnitude * minShotFactor) * _perfectShotOptimalVelocity.normalized;
        _maxShotVelocity = (_backboardShotOptimalVelocity.magnitude * maxShotFactor) * _backboardShotOptimalVelocity.normalized;
    }

    private bool TryGetPerfectVelocity(Vector3 start, Vector3 target, float targetTangentAngle, out Vector3 initialVelocity)
    {
        initialVelocity = Vector3.zero;

        Vector3 toTarget = target - start;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float distanceXZ = toTargetXZ.magnitude;
        float deltaY = toTarget.y;

        if (Mathf.Approximately(distanceXZ, 0))
            return false;

        float targetTangentAngleRad = targetTangentAngle * Mathf.Deg2Rad;
        float tanImpact = Mathf.Tan(targetTangentAngleRad);

        // Compute required launch angle
        float tanLaunch = (2f * deltaY / distanceXZ) - tanImpact;
        float launchAngleRad = Mathf.Atan(tanLaunch);

        float cosLaunch = Mathf.Cos(launchAngleRad);
        float tanLaunchVal = Mathf.Tan(launchAngleRad);

        // Denominator of speed formula
        float denom = cosLaunch * cosLaunch * (tanLaunchVal - tanImpact);
        if (denom <= 0f)
            return false; // no real solution (geometry impossible)

        // Compute required speed magnitude
        float speed = Mathf.Sqrt((_gravityMagnitude * distanceXZ) / denom);

        // Direction in XZ plane
        Vector3 dirXZ = toTargetXZ.normalized;

        // Final 3D velocity vector
        initialVelocity = cosLaunch * speed * dirXZ + Mathf.Sin(launchAngleRad) * speed * Vector3.up;
        return true;
    }

    private void OnDestroy()
    {
        onBallShot.RemoveAllListeners();
        onPositionReset.RemoveAllListeners();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debug)
            return;

        if (ball == null || court == null)
            return;

        // Ball trajectory Gizmos
        var target = court.GetHoopTarget(transform.position, previewShotType);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(ball.transform.position, target);
        Gizmos.DrawWireSphere(target, .18f);

        Gizmos.color = Color.red;
        var delta = target - ball.transform.position;
        var angleCorrection = previewShotType == ShotAimMode.Perfect ? perfectShotTangentAngle : backboardShotTangentAngle;
        Gizmos.DrawLine(target, target + Quaternion.AngleAxis(angleCorrection, Vector3.Cross(delta, Vector3.up)) * ball.transform.forward);
    }
#endif
}

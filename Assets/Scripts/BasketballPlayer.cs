using System.Collections.Generic;
using UnityEngine;

public enum ShotType
{
    Perfect,
    Backboard
}

public class BasketballPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody ballRigidbody;
    [SerializeField] private BasketballCourt court;
    [SerializeField] private PlayerInputProvider inputProvider;

    [Header("Parameters")]
    [Range(-90f, 0f)]
    [SerializeField] private float perfectShotTangentAngle = -55f;
    [Range(-90f, 0f)]
    [SerializeField] private float backboardShotTangentAngle = -20f;
    [Range(.5f, 1f)]
    [SerializeField] private float minShotFactor = .9f;
    [Range(1f, 1.2f)]
    [SerializeField] private float maxShotFactor = 1.05f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private ShotType previewShotType = ShotType.Perfect;
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
    private Pose _defaultPose;
    private MultiInterpolator<Vector3> _interpolator;

    private void Awake()
    {
        // #TODO: Find a smarter way to generalize this repeated check pattern.
        if (ballRigidbody == null)
        {
            ballRigidbody = GetComponentInChildren<Rigidbody>();
            if(ballRigidbody == null)
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

        _defaultPose.position = ballRigidbody.transform.localPosition;
        _defaultPose.rotation = ballRigidbody.transform.localRotation;

        ResetBall();

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

        // Debug
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
        }
    }

    public void OnInputReceived(float inputVelocity)
    {
        if (debug)
            Time.timeScale = timeDilationFactor;

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

        ballRigidbody.transform.parent = null;
        ballRigidbody.isKinematic = false;
        ballRigidbody.AddForce(initialVelocity, ForceMode.VelocityChange);
        ballRigidbody.AddTorque(-ballRigidbody.transform.right * Random.Range(0, 1f));
    }

    private void ResetBall()
    {
        if (debug)
            Time.timeScale = 1;

        ballRigidbody.isKinematic = true;
        var ballTransform = ballRigidbody.transform;
        ballTransform.parent = transform;
        ballTransform.SetLocalPositionAndRotation(_defaultPose.position, _defaultPose.rotation);

        _computeVelocities = true;
    }

    private void UpdateShotOptimalVelocities()
    {
        var start = ballRigidbody.transform.position;

        var backboardShotTarget = court.GetHoopTarget(transform.position, ShotType.Backboard);
        var perfectShotTarget = court.GetHoopTarget(transform.position, ShotType.Perfect);

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
        initialVelocity = dirXZ * speed * cosLaunch + Vector3.up * speed * Mathf.Sin(launchAngleRad);
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debug)
            return;

        if (ballRigidbody == null || court == null)
            return;

        // Ball trajectory Gizmos
        var target = court.GetHoopTarget(transform.position, previewShotType);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(ballRigidbody.position, target);
        Gizmos.DrawWireSphere(target, .18f);

        Gizmos.color = Color.red;
        var delta = target - ballRigidbody.position;
        var angleCorrection = previewShotType == ShotType.Perfect ? perfectShotTangentAngle : backboardShotTangentAngle;
        Gizmos.DrawLine(target, target + Quaternion.AngleAxis(angleCorrection, Vector3.Cross(delta, Vector3.up)) * ballRigidbody.transform.forward);
    }
#endif
}

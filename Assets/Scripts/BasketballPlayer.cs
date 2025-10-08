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

    [Header("Parameters")]
    [SerializeField] private float perfectShotAngleCorrection = 45f;
    [SerializeField] private float backboardShotAngleCorrection = 20f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private ShotType previewShotType = ShotType.Perfect;
    [Range(0f, 15f)]
    [SerializeField] private float testVelocity = 8f;
    [Range(0.1f, 1f)]
    [SerializeField] private float timeDilationFactor = 1f;
    
    private float _gravityMagnitude;
    private Pose _defaultPose;
    private MultiInterpolator<Vector3> _interpolator;
    private Vector3 _perfectShotOptimalVelocity, _backboardShotOptimalVelocity;
    public Vector3 PerfectShotOptimalVelocity => _perfectShotOptimalVelocity;
    public Vector3 BackboardShotOptimalVelocity => _backboardShotOptimalVelocity;

    private void Awake()
    {
        if(ballRigidbody == null)
        {
            ballRigidbody = GetComponentInChildren<Rigidbody>();
            if(ballRigidbody == null)
            {
                throw new UnassignedReferenceException("Unable to retrieve ballRigidbody reference");
            }
        }

        if(court == null)
        {
            court = FindObjectOfType<BasketballCourt>();
            if(court == null)
            {
                throw new UnassignedReferenceException("Unable to retrieve court reference");
            }
        }

        ballRigidbody.isKinematic = true;

        _defaultPose.position = ballRigidbody.transform.localPosition;
        _defaultPose.rotation = ballRigidbody.transform.localRotation;

        // Caching gravity magnitude to spare square root computing
        _gravityMagnitude = Physics.gravity.magnitude;       
        
        _interpolator = new MultiInterpolator<Vector3>(new List<KeyValuePair<float, Vector3>> 
        {
            new(0f, Vector3.zero),
            new(15*15, Vector3.zero),
        }, Vector3.Lerp);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
        }

        UpdateShotOptimalVelocities();
    }

    public void OnInputReceived(float inputVelocity)
    {
        if (debug)
            Time.timeScale = timeDilationFactor;

        _interpolator.SetPairs(new List<KeyValuePair<float, Vector3>>
        {
            new(0f, Vector3.zero),
            new(_perfectShotOptimalVelocity.sqrMagnitude, _perfectShotOptimalVelocity),
            new(_backboardShotOptimalVelocity.sqrMagnitude, _backboardShotOptimalVelocity),
            new(15f*15f, 15f*_backboardShotOptimalVelocity.normalized),
        });

        var velocity = _interpolator.Evaluate(inputVelocity * inputVelocity);
        Debug.Log($"[{GetType().Name}] Perfect Shot opt. = {_perfectShotOptimalVelocity.magnitude}, Backboard Shot opt. = {_backboardShotOptimalVelocity.magnitude}\n Velocity = {velocity.magnitude}");

        ShootBall(velocity);
    }

    private void UpdateShotOptimalVelocities()
    {
        var start = ballRigidbody.transform.position;

        var backboardShotTarget = court.GetHoopTarget(transform.position, ShotType.Backboard);
        var perfectShotTarget = court.GetHoopTarget(transform.position, ShotType.Perfect);

        var deltaPosition = perfectShotTarget - start;
        var deltaToGroundAngle = Vector3.Angle(Vector3.ProjectOnPlane(deltaPosition, Vector3.up), deltaPosition);

        TryGetPerfectVelocity(start, perfectShotTarget,
            Mathf.Clamp(deltaToGroundAngle + perfectShotAngleCorrection, 0, 90),
            out _perfectShotOptimalVelocity);

        TryGetPerfectVelocity(start, backboardShotTarget,
            Mathf.Clamp(deltaToGroundAngle + backboardShotAngleCorrection, 0, 90),
            out _backboardShotOptimalVelocity);
    }

    private void ShootBall(Vector3 initialVelocity)
    {
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
    }

    private bool TryGetPerfectVelocity(Vector3 start,  Vector3 target, float angleDeg, out Vector3 initialVelocity)
    {
        initialVelocity = Vector3.zero;
        Vector3 toTarget = target - start;

        // Horizontal displacement (XZ-plane)
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float distanceXZ = toTargetXZ.magnitude;
        float deltaY = toTarget.y;

        float angleRad = angleDeg * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angleRad);
        float tanAngle = Mathf.Tan(angleRad);

        // Check for valid geometry
        float denom = 2f * cosAngle * cosAngle * (distanceXZ * tanAngle - deltaY);
        if (denom <= 0f)
        {
            Debug.LogWarning($"[{GetType().Name}]No valid shot solution for given parameters.");
            return false;
        }

        // Compute required launch speed magnitude
        float speed = Mathf.Sqrt((_gravityMagnitude * distanceXZ * distanceXZ) / denom);

        // Construct velocity direction in XZ-plane
        Vector3 dirXZ = toTargetXZ.normalized;

        // Final velocity vector
        initialVelocity = dirXZ * speed * cosAngle + Vector3.up * speed * Mathf.Sin(angleRad);
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
        var angleCorrection = previewShotType == ShotType.Perfect ? perfectShotAngleCorrection : backboardShotAngleCorrection;
        Gizmos.DrawLine(ballRigidbody.position, ballRigidbody.position + Quaternion.AngleAxis(angleCorrection, Vector3.Cross(delta, Vector3.up)) * delta.normalized);
    }
#endif
}

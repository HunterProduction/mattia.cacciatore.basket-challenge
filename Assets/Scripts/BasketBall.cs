using System;
using Unity.VisualScripting;
using UnityEngine;

public class BasketBall : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float angleCorrection = 45f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [Range(0.1f, 1f)]
    [SerializeField] private float timeDilationFactor = 1f;

    private Rigidbody _rigidbody;
    private float _gravityMagnitude;
    private Pose _defaultPose;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;

        _defaultPose.position = transform.localPosition;
        _defaultPose.rotation = transform.localRotation;

        // Caching gravity magnitude to spare square root computing
        _gravityMagnitude = Physics.gravity.magnitude;
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPosition();
        }
    }

    private void Shoot()
    {
        if (debug)
            Time.timeScale = timeDilationFactor;

        var deltaPosition = target.position - transform.position;

        /*
        var tOpt = Mathf.Sqrt(2f * deltaPosition.magnitude / _gravityMagnitude);
        var velocity = deltaPosition / tOpt - 0.5f * tOpt * Physics.gravity;
        */

        var velocity = Vector3.zero;

        var deltaToGroundAngle = Vector3.Angle(Vector3.ProjectOnPlane(deltaPosition, Vector3.up), deltaPosition);


        TryGetPerfectVelocity(transform.position, target.position, Mathf.Clamp(deltaToGroundAngle + angleCorrection, 0, 90), out velocity);

        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(velocity, ForceMode.VelocityChange);
    }

    private void ResetPosition()
    {
        if (debug)
            Time.timeScale = 1;

        _rigidbody.isKinematic = true;
        transform.SetLocalPositionAndRotation(_defaultPose.position, _defaultPose.rotation);
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
            Debug.LogWarning($"[{GetType().Name}]No valid solution for given parameters.");
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

    private void OnDrawGizmos()
    {
        if (!debug)
            return;

        if (target == null)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, target.position);

        Gizmos.color = Color.red;
        var delta = target.position - transform.position;
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(angleCorrection, Vector3.Cross(delta, Vector3.up)) * delta.normalized);
    }
}

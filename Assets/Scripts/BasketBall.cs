using System;
using Unity.VisualScripting;
using UnityEngine;

public class BasketBall : MonoBehaviour
{
    [SerializeField] private Transform target;

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
        var deltaPosition = target.position - transform.position;

        /*
        var tOpt = Mathf.Sqrt(2f * deltaPosition.magnitude / _gravityMagnitude);
        var velocity = deltaPosition / tOpt - 0.5f * tOpt * Physics.gravity;
        */

        var velocity = Vector3.zero;
        TryGetPerfectVelocity(transform.position, target.position, 45, out velocity);

        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(velocity, ForceMode.VelocityChange);
    }

    private void ResetPosition()
    {
        _rigidbody.isKinematic = true;
        transform.SetLocalPositionAndRotation(_defaultPose.position, _defaultPose.rotation);
    }

    private bool TryGetPerfectVelocity(Vector3 start,  Vector3 target, float targetImpactAngle, out Vector3 initialVelocity)
    {
        //TODO: Here we assume a throwing angle of 45°. Refactor math to allow a more custom angle.
        initialVelocity = Vector3.zero;

        var delta = target - start;
        var deltaHorizontal = new Vector2(delta.x, delta.z).magnitude;
        var deltaY = delta.y;

        var denom = deltaHorizontal - deltaY;
        if (deltaHorizontal <= 1e-6f || denom <= 0f)
            return false;

        var v0 = Mathf.Sqrt(_gravityMagnitude * deltaHorizontal * deltaHorizontal / denom);

        var horizontalDirection = new Vector3(delta.x, 0, delta.z)/deltaHorizontal;
        var vComponent = v0 / Mathf.Sqrt(2);
        initialVelocity = vComponent*horizontalDirection + vComponent*Vector3.up;

        return true;
    }
}

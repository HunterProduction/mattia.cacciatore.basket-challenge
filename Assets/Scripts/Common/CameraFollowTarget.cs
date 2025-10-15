using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowTarget : MonoBehaviour
{ 
    [SerializeField] private Transform target;

    [Range(0f, 1f)] public float positionSmoothness = 0.1f;
    [Range(0f, 1f)] public float rotationSmoothness = 0.1f;
    public bool useDamping = true;
    public UpdateMethod updateMethod = UpdateMethod.LateUpdate;

    private Vector3 _positionVelocity;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (target == null)
            return;

        SnapToTarget();
    }
#endif

    private void Awake()
    {
        if (target == null)
            return;

        SnapToTarget();
    }

    private void Update()
    {
        if (updateMethod != UpdateMethod.Update)
            return;
        UpdateCameraFollow();
    }

    private void FixedUpdate()
    {
        if (updateMethod != UpdateMethod.FixedUpdate)
            return;
        UpdateCameraFollow();
    }

    private void LateUpdate()
    {
        if (updateMethod != UpdateMethod.LateUpdate)
            return;
        UpdateCameraFollow();
    }

    private void UpdateCameraFollow()
    {
        if (target == null)
            return;

        if (useDamping)
        {
            transform.SetPositionAndRotation(Vector3.SmoothDamp(
                transform.position,
                target.position,
                ref _positionVelocity,
                1f - positionSmoothness // smaller = snappier
            ), Quaternion.Slerp(
                transform.rotation,
                target.rotation,
                1f - Mathf.Exp(-rotationSmoothness * Time.deltaTime * 60f)
            ));
        }
        else
        {
            transform.SetPositionAndRotation(Vector3.Lerp(
                transform.position,
                target.position,
                positionSmoothness
            ), Quaternion.Slerp(
                transform.rotation,
                target.rotation,
                rotationSmoothness
            ));
        }
    }

    public void SetTarget(Transform newTarget, bool snap = false)
    {
        target = newTarget;
        if(snap) SnapToTarget();
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        _positionVelocity = Vector3.zero;
        transform.SetPositionAndRotation(target.position, target.rotation);
    }
}

using UnityEngine;

public class BasketballCameraTarget : MonoBehaviour
{
    [Header("Focus")]
    public Transform ballTransform;

    [Header("Reference")]
    [SerializeField] private BasketballPlayer player;

    [Header("Parameters")]
    public float armLength = 2f;
    public float armPitchCorrection = 20f;
    public float lookAtTargetPitchCorrection = 0f;
    [SerializeField] private UpdateMethod updateMethod;

    private void Awake()
    {
        transform.parent = null;
        UpdatePositionAndRotation();
    }

    private void Update()
    {
        if (updateMethod != UpdateMethod.Update)
            return;
        UpdatePositionAndRotation();
    }

    private void FixedUpdate()
    {
        if (updateMethod != UpdateMethod.FixedUpdate)
            return;
        UpdatePositionAndRotation();
    }

    private void LateUpdate()
    {
        if (updateMethod != UpdateMethod.LateUpdate)
            return;
        UpdatePositionAndRotation();
    }

    private void UpdatePositionAndRotation()
    {
        var playerTransform = player.transform;
        var armDirection = Quaternion.AngleAxis(armPitchCorrection, playerTransform.right) * -playerTransform.forward;

        transform.position = ballTransform.position + armLength*armDirection;

        var deltaPosition = ballTransform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(deltaPosition, playerTransform.up) * Quaternion.AngleAxis(lookAtTargetPitchCorrection,  Vector3.right);
    }
}

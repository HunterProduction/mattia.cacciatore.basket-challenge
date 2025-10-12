using UnityEngine;

public class BasketballCameraTarget : MonoBehaviour
{

    public Transform ballTransform;
    [SerializeField] private BasketballPlayer player;

    public float armLength = 2f;
    public float armPitchCorrection = 20f;
    public float lookAtTargetPitchCorrection = 0f;

    private void Awake()
    {
        transform.parent = null;
        UpdatePositionAndRotation();
    }

    private void LateUpdate()
    {
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

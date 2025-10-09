using UnityEngine;

public class BasketballCourt : MonoBehaviour
{
    public enum ShootingArea
    {
        Left,
        Center,
        Right,
        OutOfBounds
    }

    [Header("Hoop")]
    [SerializeField] private Transform hoopBaseTransform;
    [SerializeField] private Transform hoopCenterTarget;
    [SerializeField] private Transform backboardCenterTarget, backboardLeftTarget, backboardRightTarget;
    [Range(0f, 90f)]
    [SerializeField] private float shootingAreaAngle = 60f;

    public Transform HoopBase => hoopBaseTransform;

    [Header("Debug")]
    [SerializeField] private bool debug;

    public Vector3 GetHoopTarget(Vector3 playerPosition, ShotType shotType)
    {
        if(shotType == ShotType.Perfect)
        {
            return hoopCenterTarget.position;
        }
        else
        {
            return GetCurrentPlayerShootingArea(playerPosition) switch
            {
                ShootingArea.Left => backboardLeftTarget.position,
                ShootingArea.Right => backboardRightTarget.position,
                ShootingArea.Center => backboardCenterTarget.position,
                _ => Vector3.zero
            };
        }
    }

    public ShootingArea GetCurrentPlayerShootingArea(Vector3 playerPosition)
    {
        var angle = Vector3.SignedAngle(hoopBaseTransform.right, playerPosition - hoopBaseTransform.position, -hoopBaseTransform.up);

        if (angle < 0f)
            return ShootingArea.OutOfBounds;

        if (angle <= shootingAreaAngle)
            return ShootingArea.Left;
        else if (angle <= 180 - shootingAreaAngle)
            return ShootingArea.Center;
        else
            return ShootingArea.Right;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debug)
            return;

        if (hoopCenterTarget == null ||
            backboardCenterTarget == null ||
            backboardLeftTarget == null ||
            hoopBaseTransform == null)
            return;
        
        // Court Gizmos
        Gizmos.color = Color.yellow;
        var length = 8f;
        Gizmos.DrawLine(hoopBaseTransform.position, hoopBaseTransform.position + hoopBaseTransform.right * length);
        Gizmos.DrawLine(hoopBaseTransform.position, hoopBaseTransform.position + Quaternion.AngleAxis(shootingAreaAngle, -hoopBaseTransform.up) * hoopBaseTransform.right * length);
        Gizmos.DrawLine(hoopBaseTransform.position, hoopBaseTransform.position + Quaternion.AngleAxis(-shootingAreaAngle, -hoopBaseTransform.up) * -hoopBaseTransform.right * length);
        Gizmos.DrawLine(hoopBaseTransform.position, hoopBaseTransform.position + -hoopBaseTransform.right * length);
    }
#endif
}

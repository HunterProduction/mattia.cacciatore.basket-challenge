using TMPro;
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

    [SerializeField] private BasketballHoop hoop;
    [SerializeField] private float shootingAreaAngle = 60f;

    [Header("Debug")]
    [SerializeField] private bool debug;

    public Vector3 GetHoopTarget(Vector3 playerPosition, ShotType shotType)
    {
        if(shotType == ShotType.Perfect)
        {
            return hoop.PerfectTarget.position;
        }
        else
        {
            return GetCurrentPlayerShootingArea(playerPosition) switch
            {
                ShootingArea.Left => hoop.BackboardTargetLeft.position,
                ShootingArea.Right => hoop.BackboardTargetRight.position,
                ShootingArea.Center => hoop.BackboardTargetCenter.position,
                _ => Vector3.zero
            };
        }
    }

    public ShootingArea GetCurrentPlayerShootingArea(Vector3 playerPosition)
    {
        var hoopBase = hoop.Base;
        var angle = Vector3.SignedAngle(hoopBase.right, playerPosition - hoopBase.position, -hoopBase.up);

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

        if (hoop == null)
            return;
        
        // Court Gizmos
        Gizmos.color = Color.yellow;
        var length = 8f;
        var hoopBase = hoop.Base;
        Gizmos.DrawLine(hoopBase.position, hoopBase.position + hoopBase.right * length);
        Gizmos.DrawLine(hoopBase.position, hoopBase.position + Quaternion.AngleAxis(shootingAreaAngle, -hoopBase.up) * hoopBase.right * length);
        Gizmos.DrawLine(hoopBase.position, hoopBase.position + Quaternion.AngleAxis(-shootingAreaAngle, -hoopBase.up) * -hoopBase.right * length);
        Gizmos.DrawLine(hoopBase.position, hoopBase.position + -hoopBase.right * length);
    }
#endif
}

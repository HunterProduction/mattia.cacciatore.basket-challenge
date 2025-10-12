using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BasketballCourt : MonoBehaviour
{
    public enum ShootingArea
    {
        Left,
        Center,
        Right,
        OutOfBounds
    }
    [Header("References")]
    [SerializeField] private BasketballHoop hoop;

    [Header("Configuration")]
    [SerializeField] private float shootingAreaAngle = 60f;
    [SerializeField] private Transform[] shootPositions;

    [Header("Debug")]
    [SerializeField] private bool debug;

    private Dictionary<int, int> _playersCurrentPositionMap;

    private void Start()
    {
        _playersCurrentPositionMap = new Dictionary<int, int>();

        foreach(var player in BasketballGameManager.Instance.Players)
        {
            _playersCurrentPositionMap.Add(player.GetInstanceID(), 0);
            player.ResetPlayer(shootPositions[0].position);
        }
    }

    public void SetPlayerNextPosition(BasketballPlayer player)
    {
        var playerId = player.GetInstanceID();
        var nextPosIndex = _playersCurrentPositionMap[playerId];

        // Find the next free shoot position.
        do nextPosIndex = (nextPosIndex + 1) % shootPositions.Length;
        while (_playersCurrentPositionMap.ContainsValue(nextPosIndex));

        player.ResetPlayer(shootPositions[nextPosIndex].position);
        _playersCurrentPositionMap[playerId] = nextPosIndex;
    }

    public Vector3 GetHoopTarget(Vector3 playerPosition, BasketballPlayer.ShotAimMode shotType)
    {
        if(shotType == BasketballPlayer.ShotAimMode.Perfect)
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

        if (shootPositions == null)
            return;

        // Shoot Positions
        Handles.color = Color.yellow;
        for (int i = 0; i < shootPositions.Length; i++)
        {
            Handles.Label(shootPositions[i].position+Vector3.up*.1f, (i+1).ToString());
            Handles.DrawWireDisc(shootPositions[i].position, Vector3.up, .2f);
        }

        if (hoop == null)
            return;
        // Shooting Area Gizmos
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

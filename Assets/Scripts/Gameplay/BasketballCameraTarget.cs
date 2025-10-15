using System;
using UnityEngine;

public class BasketballCameraTarget : MonoBehaviour
{
    [Header("Focus")]
    [SerializeField] private CameraFollowTarget cameraFollow;
    public Transform ballTransform;

    [Header("Reference")]
    [SerializeField] private BasketballPlayer player;

    [Header("Parameters")]
    public float armLength = 2f;
    public float armPitchCorrection = 20f;
    public float lookAtTargetPitchCorrection = 0f;
    [SerializeField] private UpdateMethod updateMethod;

    private BasketballGameManager _gameManager;

    private void Start()
    {
        transform.parent = null;

        if (cameraFollow == null)
            cameraFollow = Camera.main.GetComponent<CameraFollowTarget>();

        SetCustomCameraTarget(null, true);
        UpdatePositionAndRotation();

        player.onPositionReset.AddListener(() => enabled = true);
        _gameManager = BasketballGameManager.Instance;
        _gameManager.onPointScored.AddListener(OnPointScored);
        _gameManager.onGameOver.AddListener(OnGameOver);
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

    private void OnPointScored(PointScoredArgs args)
    {
        if (args.shotData.player == player)
            enabled = false;
    }

    private void OnGameOver(GameOverArgs result)
    {
        SetCustomCameraTarget(result.winner.EndGameCameraTarget);
    }

    private void OnDestroy()
    {
        if(_gameManager != null)
        {
            _gameManager.onPointScored.RemoveListener(OnPointScored);
        }

        if (player != null)
            player.onPositionReset.RemoveListener(() => enabled = true);
    }

    public void SetCustomCameraTarget(Transform targetTransform, bool snap = false)
    {
        cameraFollow.SetTarget(targetTransform == null ? transform : targetTransform, snap);
    }
}

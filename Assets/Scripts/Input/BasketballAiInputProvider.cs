using UnityEngine;

public class BasketballAiInputProvider : BasketballInputProvider
{
    [Header("Data")]
    [SerializeField] private AiBasketballPlayerData aiData;

    private float _timeElapsed, _currentAimTime;

    /**
     * #NOTE: #MattiaCacciatore The logic of entering-exiting aiming mode could be better formalized into a State Machine pattern,
     * if the AI complexity is meant to be more versatile and scalable to more complex tasks. Given the context of this test prototype,
     * the architecture has been simplified to a simple boolean flag.
     */
    private bool _aiming;
    private void SetAiming(bool value)
    {
        if (value)
        {
            var randomFactor = Random.Range(1 - aiData.aimTimeVariancePercentage, 1 + aiData.aimTimeVariancePercentage);
            _currentAimTime = aiData.aimTime * randomFactor;
            _aiming = true;
        }
        _aiming = value;
    }

    protected override void Awake()
    {
        base.Awake();
        player.onPositionReset.AddListener(OnPositionReset);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Debug.Log($"[{GetType().Name}] Ai Input enabled");
        SetAiming(true);
    }


    private void Update()
    {
        if (!_aiming)
            return;

        _timeElapsed += Time.deltaTime;
        if (_timeElapsed >= _currentAimTime)
        {
            Debug.Log($"[{GetType().Name}] Shooting");
            _aiming = false;

            var min = player.MinShotVelocity.sqrMagnitude;
            var max = player.MaxShotVelocity.sqrMagnitude;
            var perfectOptimal = player.PerfectShotOptimalVelocity.sqrMagnitude;
            var backboardOptimal = player.BackboardShotOptimalVelocity.sqrMagnitude;

            // choose whether to aim for backboard or perfect shot based on preference
            bool chooseBackboard = Random.value < aiData.perfectOrBackboardShotPreference;
            float chosenOptimal = chooseBackboard ? backboardOptimal : perfectOptimal;

            float randomCandidate = Random.Range(min, max);

            // blend between random candidate and optimal using accuracy as weight
            float initialVelocitySquared = Mathf.Lerp(randomCandidate, chosenOptimal, aiData.accuracy);

            _currentValueSquared = Mathf.Clamp(initialVelocitySquared, min, max);
            SendInput();
        }
    }

    private void OnPositionReset()
    {
        _timeElapsed = 0f;
        SetAiming(true);
    }

    private void OnDisable()
    {
        Debug.Log($"[{GetType().Name}] Ai Input disabled");
        SetAiming(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(player != null)
            player.onPositionReset?.RemoveListener(OnPositionReset);
    }
}

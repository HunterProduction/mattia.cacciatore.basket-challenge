using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RandomBonusEventsDispatcher : MonoBehaviour
{
    [SerializeField] private GameConfigData gameConfig;

    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private SendEventMode sendEventMode;

    // #NOTE: #MattiaCacciatore With a custom inspector/drawer/attribute, this field could be hided based on sendEventMode value.
    [Header("Unity Events")]
    public UnityEvent<ShotScoreBonus> onBonusEventTriggered;

    public event Action<ShotScoreBonus> bonusEventTriggered;

    private float _nextEventTimePeriod;
    private float _timeElapsed;
    private float _totalRate;   // Λ = sum(λ_i)

    private void Start()
    {
        if (gameConfig == null)
            gameConfig = BasketballGameManager.Instance.GameConfig;
        ComputeTotalRate();
        ScheduleNext();

        _timeElapsed = 0;
        if (initialDelay > 0f)
            StartCoroutine(StartupDelayedCoroutine());
    }

    private void Update()
    {
        if (_timeElapsed >= _nextEventTimePeriod)
        {
            var bonus = PickBonus();

            if (sendEventMode == SendEventMode.UnityEvent || sendEventMode == SendEventMode.Both)
                onBonusEventTriggered?.Invoke(bonus);
            bonusEventTriggered?.Invoke(bonus);

            ScheduleNext();
            _timeElapsed = 0f;
        }
        _timeElapsed += Time.deltaTime;
    }

    private void ComputeTotalRate()
    {
        _totalRate = 0f;
        foreach (var bonus in gameConfig.backboardBonuses)
        {
            var rate = bonus.Rarity.GetFrequencyPerSecond();
            if (rate > 0f)
                _totalRate += rate;
        }
    }

    private void ScheduleNext()
    {
        // Sample exponential waiting time for next event
        _nextEventTimePeriod = -Mathf.Log(UnityEngine.Random.value) / _totalRate;
    }

    private ShotScoreBonus PickBonus()
    {
        float rate = UnityEngine.Random.value * _totalRate;
        float cumulative = 0f;

        foreach (var bonus in gameConfig.backboardBonuses)
        {
            cumulative += bonus.Rarity.GetFrequencyPerSecond();
            if (rate <= cumulative)
                return bonus;
        }

        // fallback (shouldn't happen due to floating-point rounding)
        return gameConfig.backboardBonuses[^1];
    }

    private IEnumerator StartupDelayedCoroutine()
    {
        this.enabled = false;
        var wait = new WaitForSeconds(initialDelay);
        yield return wait;
        this.enabled = true;
    }

    private void OnDestroy()
    {
        onBonusEventTriggered?.RemoveAllListeners();
    }
}


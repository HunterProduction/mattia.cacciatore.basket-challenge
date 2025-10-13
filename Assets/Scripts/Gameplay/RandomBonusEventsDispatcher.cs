using System;
using UnityEngine;
using UnityEngine.Events;

public class RandomBonusEventsDispatcher : MonoBehaviour
{
    [SerializeField] private ScoreBonus[] bonuses;
    [SerializeField] private SendEventMode sendEventMode;

    // #NOTE: #MattiaCacciatore With a custom inspector/drawer/attribute, this field could be hided based on sendEventMode value.
    [Header("Unity Events")]
    public UnityEvent<ScoreBonus> onBonusEventTriggered;

    public event Action<ScoreBonus> bonusEventTriggered;

    private float _nextEventTimePeriod;
    private float _timeElapsed;
    private float _totalRate;   // Λ = sum(λ_i)

    private void Start()
    {
        ComputeTotalRate();
        ScheduleNext();

        _timeElapsed = 0;
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
        foreach (var bonus in bonuses)
        {
            var rate = bonus.rarity.GetFrequencyPerSecond();
            if (rate > 0f)
                _totalRate += rate;
        }
    }

    private void ScheduleNext()
    {
        // Sample exponential waiting time for next event
        _nextEventTimePeriod = -Mathf.Log(UnityEngine.Random.value) / _totalRate;
    }

    private ScoreBonus PickBonus()
    {
        float rate = UnityEngine.Random.value * _totalRate;
        float cumulative = 0f;

        foreach (var bonus in bonuses)
        {
            cumulative += bonus.rarity.GetFrequencyPerSecond();
            if (rate <= cumulative)
                return bonus;
        }

        // fallback (shouldn't happen due to floating-point rounding)
        return bonuses[^1];
    }

    private void OnDestroy()
    {
        onBonusEventTriggered?.RemoveAllListeners();
    }
}


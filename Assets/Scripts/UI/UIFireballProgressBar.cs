using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFireballProgressBar : MonoBehaviour
{
    [SerializeField] private FireballBonusDispatcher fireballBonusDispatcher;

    [Header("UI Elements")]
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject bonusNotification;

    private void OnEnable()
    {
        if (fireballBonusDispatcher == null)
        {
            var player = BasketballGameManager.Instance.GetUserPlayer();
            if(!player.Ball.TryGetComponent(out fireballBonusDispatcher))
            {
                Debug.LogError($"[{GetType().Name}] Missing FireballBonusDispatcher reference", this);
                return;
            }            
        }

        if(bonusNotification != null)
        {
            bonusNotification.SetActive(false);
            fireballBonusDispatcher.onFireballBonusStarted.AddListener(OnBonusStarted);
            fireballBonusDispatcher.onFireballBonusReset.AddListener(OnBonusReset);
        }

        slider.maxValue = fireballBonusDispatcher.Threshold;
    }

    private void Update()
    {
        slider.value = fireballBonusDispatcher.CurrentStreak;
    }

    private void OnDisable()
    {
        if (fireballBonusDispatcher != null)
        {
            fireballBonusDispatcher.onFireballBonusStarted.RemoveListener(OnBonusStarted);
            fireballBonusDispatcher.onFireballBonusReset.RemoveListener(OnBonusReset);
        }
    }

    private void OnBonusReset()
    {
        bonusNotification.SetActive(false);
    }

    private void OnBonusStarted()
    {
        bonusNotification.SetActive(true);
    }
}

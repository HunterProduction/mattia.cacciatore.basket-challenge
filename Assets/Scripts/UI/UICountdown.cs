using System;
using TMPro;
using UnityEngine;

public class UICountdown : MonoBehaviour
{
    [SerializeField] private int countdownTime = 0;
    [SerializeField] private bool activateOnStart;
    [SerializeField] private bool deactivateOnEnd;

    [Header("UI")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private string format = "mm\\:ss";

    public int CountdownTime
    {
        get => countdownTime;
        set
        {
            countdownTime = value;
            if(!_running && _timeRemaining<=0 && text != null)
                text.text = TimeSpan.FromSeconds(countdownTime).ToString(format);
        }
    }

    public event Action started, paused, stopped;

    private float _timeRemaining;
    public int TimeRemaining => Mathf.CeilToInt(_timeRemaining);

    private bool _running;

    private void Start()
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>();
        text.text = TimeSpan.FromSeconds(countdownTime).ToString(format);
        if (activateOnStart)
        {
            StartCountdown();
        }
    }

    public void StartCountdown()
    {
        if (_running)
            return;

        if(_timeRemaining <= 0)
            _timeRemaining = countdownTime;
        _running = true;
        started?.Invoke();
    }

    public void PauseCountdown()
    {
        if (_running)
            _running = false;
        paused?.Invoke();
    }

    public void StopCountdown()
    {
        PauseCountdown();
        _timeRemaining = 0;
        stopped?.Invoke();
    }

    private void Update()
    {
        if(!_running)
            return;

        /** 
         * #NOTE: #MattiaCacciatore Not the best to allocate a TimeSpan every frame, but it's convenient for string formatting
         */
        text.text = TimeSpan.FromSeconds(TimeRemaining).ToString(format);

        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
        }
        else
        {
            StopCountdown();
            if (deactivateOnEnd)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

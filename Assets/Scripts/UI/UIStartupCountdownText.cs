using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class UIStartupCountdownText : MonoBehaviour
{
    [SerializeField] private bool deactivateOnEnd;

    private TMP_Text _text;
    private float _timeRemaining;

    private void Start()
    {
        _timeRemaining = BasketballGameManager.Instance.StartupCountdownTime;

        _text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        _text.text = Mathf.CeilToInt(_timeRemaining).ToString();

        if (_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
        }
        else
        {
            if (deactivateOnEnd)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

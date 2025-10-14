using TMPro;
using UnityEngine;

public class UIScoredPointPopup : MonoBehaviour
{
    [SerializeField] private float showTime = 3f;

    private TMP_Text _text;
    private float _timer;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }
    private void Start()
    {
        BasketballGameManager.Instance.onPointScored.AddListener(Show);
        gameObject.SetActive(false);
    }

    public void Show(PointScoredArgs pointScoredArgs)
    {
        _text.text = "+" + pointScoredArgs.pointScored.ToString();
        _timer = showTime;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if(_timer > 0f)
            _timer -= Time.deltaTime;
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (BasketballGameManager.Instance != null)
            BasketballGameManager.Instance.onPointScored?.RemoveListener(Show);
    }
}

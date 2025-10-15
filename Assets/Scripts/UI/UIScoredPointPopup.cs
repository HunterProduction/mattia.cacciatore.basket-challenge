using TMPro;
using UnityEngine;

public class UIScoredPointPopup : MonoBehaviour
{
    [SerializeField] private float showTime = 3f;

    private TMP_Text _text;
    private float _timer;
    private BasketballGameManager _gameManager;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }
    private void Start()
    {
        _gameManager = BasketballGameManager.Instance;
        _gameManager.onPointScored.AddListener(Show);
        gameObject.SetActive(false);
    }

    public void Show(PointScoredArgs pointScoredArgs)
    {
        // Show added score popup only if it's a point of the user.
        if(!pointScoredArgs.shotData.player.IsUser)
            return;

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
        if (_gameManager != null)
            _gameManager.onPointScored?.RemoveListener(Show);
    }
}

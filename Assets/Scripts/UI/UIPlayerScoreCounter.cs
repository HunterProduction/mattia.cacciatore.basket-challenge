using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIPlayerScoreCounter : MonoBehaviour
{
    [SerializeField] private BasketballPlayer player;

    private TextMeshProUGUI _text;
    private BasketballGameManager _gameManager;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();

        // Caching game manager instance to avoid continuous singleton property access and null check.
        _gameManager = BasketballGameManager.Instance;
    }

    private void Update()
    {
        _text.text = _gameManager.GetPlayerScore(player).ToString();
    }
}

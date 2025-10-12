using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIPlayerScoreCounter : MonoBehaviour
{
    [SerializeField] private BasketballGameManager gameManager;
    [SerializeField] private BasketballPlayer player;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();

        if(gameManager == null)
        {
            gameManager = FindObjectOfType<BasketballGameManager>();
        }
    }

    private void Update()
    {
        _text.text = gameManager.GetPlayerScore(player).ToString();
    }
}

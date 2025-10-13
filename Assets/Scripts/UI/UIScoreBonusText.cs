using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class UIScoreBonusText : MonoBehaviour
{
    struct RarityColor
    {
        public EventRarity rarity;
        public Color color;
    }
    [SerializeField] private RarityColor[] rarityColors;

    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    public void UpdateText(ScoreBonus bonus)
    {
        var bonusText = bonus.value.ToString();

        // Bonus type notation
        bonusText = bonus.type switch
        {
            ScoreBonus.Type.Additive => "+" + bonusText,
            ScoreBonus.Type.Multiplicative => bonusText + "x",
            _ => throw new System.NotImplementedException(),
        };

        _text.text = bonusText;
        _text.color = bonus.rarity.GetColor();
    }
}

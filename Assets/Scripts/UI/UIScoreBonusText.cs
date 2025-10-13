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

    public void UpdateText(Bonus bonus)
    {
        var bonusText = bonus.Value.ToString();

        // Bonus type notation
        bonusText = bonus.Type switch
        {
            Bonus.ApplyType.Additive => "+" + bonusText,
            Bonus.ApplyType.Multiplicative => bonusText + "x",
            _ => throw new System.NotImplementedException(),
        };

        _text.text = bonusText;
        _text.color = bonus.Rarity.GetColor();
    }
}

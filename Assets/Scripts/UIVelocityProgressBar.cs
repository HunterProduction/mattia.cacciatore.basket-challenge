using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UIVelocityProgressBar : MonoBehaviour
{
    [SerializeField] private PlayerInputProvider playerInputProvider;
    [SerializeField] private BasketballPlayer player;

    [Header("References")]
    [SerializeField] private RelativePositionFitter perfectShotMarker;
    [SerializeField] private RelativePositionFitter backboardShotMarker;
    [SerializeField] private RelativePositionFitter currentValueMarker;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Update()
    {
        UpdateVisuals(playerInputProvider.CurrentValue);
    }

    private void UpdateVisuals(float currentVelocity)
    {
        var min = playerInputProvider.minInputVelocity;
        var max = playerInputProvider.maxInputVelocity;

        _slider.maxValue = max;
        _slider.minValue = min;
        _slider.value = currentVelocity;

        currentValueMarker.Percent = new Vector2(0.5f, (currentVelocity-min)/(max-min));

        perfectShotMarker.Percent = new Vector2(0.5f, (player.PerfectShotOptimalVelocity.magnitude-min) / (max-min));
        backboardShotMarker.Percent = new Vector2(0.5f, (player.BackboardShotOptimalVelocity.magnitude-min) / (max - min));
    }

}

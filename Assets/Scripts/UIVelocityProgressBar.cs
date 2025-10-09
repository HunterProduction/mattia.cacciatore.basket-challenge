using UnityEditor.Experimental.GraphView;
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

        // #TODO: Find a smarter way to generalize this repeated check pattern.
        if (player == null)
        {
            player = FindObjectOfType<BasketballPlayer>();
            if (player == null)
            {
                throw new UnassignedReferenceException("Unable to retrieve reference");
            }
        }

        if (playerInputProvider == null)
        {
            playerInputProvider = FindObjectOfType<PlayerInputProvider>();
            if (playerInputProvider == null)
            {
                throw new UnassignedReferenceException("Unable to retrieve reference");
            }
        }
    }

    private void Update()
    {
        UpdateVisuals(playerInputProvider.CurrentValue);
    }

    private void UpdateVisuals(float currentVelocity)
    {
        /*
         *  #NOTE: Where it's mathematically possible, magnitudes are kept squared to avoid
         *  repeated square root computations.
         */
        var min = player.MinShotVelocity.sqrMagnitude;
        var max = player.MaxShotVelocity.sqrMagnitude;

        _slider.maxValue = max;
        _slider.minValue = min;
        _slider.value = currentVelocity * currentVelocity;

        currentValueMarker.Percent = new Vector2(0.5f, (_slider.value - min)/(max-min));
        perfectShotMarker.Percent = new Vector2(0.5f, (player.PerfectShotOptimalVelocity.sqrMagnitude-min) / (max-min));
        backboardShotMarker.Percent = new Vector2(0.5f, (player.BackboardShotOptimalVelocity.sqrMagnitude-min) / (max - min));
    }

}

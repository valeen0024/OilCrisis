using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD References")]
    [SerializeField] private Image fuelBarFill;
    [SerializeField] private TMP_Text distanceText;

    [Header("Gameplay References")]
    [SerializeField] private KartFuelSystem playerFuelSystem;
    [SerializeField] private DistanceTracker playerDistanceTracker;

    private void Update()
    {
        UpdateFuelBar();
        UpdateDistance();
    }

    /// <summary>
    /// Updates the fuel bar based on the player's remaining fuel.
    /// </summary>
    private void UpdateFuelBar()
    {
        if (playerFuelSystem == null || fuelBarFill == null)
            return;

        fuelBarFill.fillAmount = playerFuelSystem.GetFuelNormalized();
    }

    /// <summary>
    /// Updates the distance text in real time.
    /// </summary>
    private void UpdateDistance()
    {
        if (playerDistanceTracker == null || distanceText == null)
            return;

        distanceText.text = $"{playerDistanceTracker.currentDistance:F1} YDS";
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD References")]
    [SerializeField] private Slider fuelBar;
    [SerializeField] private TMP_Text j1DistanceText;
    [SerializeField] private TMP_Text j2DistanceText;

    [Header("Gameplay References")]
    [SerializeField] private KartFuelSystem playerFuelSystem;
    [SerializeField] private KartFuelSystem cpuFuelSystem;

    [SerializeField] private DistanceTracker playerDistanceTracker;
    [SerializeField] private DistanceTracker cpuDistanceTracker;

    [Header("Winner UI")]
    [SerializeField] private GameObject uiPlay;
    [SerializeField] private GameObject uiWin;

    [SerializeField] private GameObject imgJ1;
    [SerializeField] private GameObject imgJ2;
    [SerializeField] private GameObject imgTie;

    private bool winnerShown = false;

    private void Update()
    {
        UpdateFuelBar();
        UpdateDistance();

        if (!winnerShown && GameManager.Instance.gameState == GameManager.GameState.Finished)
        {
            ShowWinnerUI();
        }
    }

    private void UpdateFuelBar()
    {
        if (fuelBar == null)
            return;

        if (GameManager.Instance.gameState == GameManager.GameState.PlayerTurn)
        {
            if (playerFuelSystem != null)
            {
                fuelBar.value = playerFuelSystem.GetFuelNormalized();
            }
        }
        else if (GameManager.Instance.gameState == GameManager.GameState.CPUTurn)
        {
            if (cpuFuelSystem != null)
            {
                fuelBar.value = cpuFuelSystem.GetFuelNormalized();
            }
        }
    }
    private void UpdateDistance()
    {
        if (playerDistanceTracker != null && j1DistanceText != null)
        {
            j1DistanceText.text = playerDistanceTracker.currentDistance.ToString("F1") + " YDS";
        }

        if (cpuDistanceTracker != null && j2DistanceText != null)
        {
            j2DistanceText.text = cpuDistanceTracker.currentDistance.ToString("F1") + " YDS";
        }
    }

    public void ShowWinnerUI()
    {
        winnerShown = true;

        uiPlay.SetActive(false);
        uiWin.SetActive(true);

        imgJ1.SetActive(false);
        imgJ2.SetActive(false);
        imgTie.SetActive(false);

        switch (GameManager.Instance.winnerName)
        {
            case "Player":
                imgJ1.SetActive(true);
                break;

            case "CPU":
                imgJ2.SetActive(true);
                break;

            case "Draw":
                imgTie.SetActive(true);
                break;
        }
    }
}
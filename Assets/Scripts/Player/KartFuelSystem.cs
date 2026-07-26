using UnityEngine;

public class KartFuelSystem : MonoBehaviour
{
    [Header("Fuel Configuration")]
    public float currentFuel = 100f;
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 10f;

    [Header("Rewards & Penalties")]
    public float fuelFromBarrel = 25f;
    public float fuelLostFromOil = 15f;

    private bool isOutOfFuel = false;

    private KartLaneController kartController;

    public bool IsOutOfFuel => isOutOfFuel;

    void Start()
    {
        kartController = GetComponent<KartLaneController>();
    }

    void Update()
    {
        if (isOutOfFuel)
            return;

        if (currentFuel > 0f)
        {
            currentFuel -= fuelConsumptionRate * Time.deltaTime;
            currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);

            if (currentFuel <= 0f)
            {
                OnFuelEmpty();
            }
        }
    }

    /// <summary>
    /// Returns the current fuel percentage between 0 and 1.
    /// </summary>
    public float GetFuelNormalized()
    {
        return currentFuel / maxFuel;
    }

    /// <summary>
    /// Adds fuel to the kart.
    /// </summary>
    public void AddFuel(float amount = -1f)
    {
        float amountToAdd = (amount < 0f) ? fuelFromBarrel : amount;

        currentFuel = Mathf.Clamp(currentFuel + amountToAdd, 0f, maxFuel);

        if (currentFuel > 0f && isOutOfFuel)
        {
            isOutOfFuel = false;

            if (kartController != null)
            {
                kartController.canMove = true;
            }

            Debug.Log("Fuel restored! Kart reactivated.");
        }
        else
        {
            Debug.Log($"Fuel refilled (+{amountToAdd})! Total: {currentFuel}");
        }
    }

    /// <summary>
    /// Reduces fuel when driving over oil.
    /// </summary>
    public void ReduceFuelFromOil()
    {
        currentFuel = Mathf.Clamp(currentFuel - fuelLostFromOil, 0f, maxFuel);

        Debug.Log($"Fuel lost because of oil! Total: {currentFuel}");

        if (currentFuel <= 0f && !isOutOfFuel)
        {
            OnFuelEmpty();
        }
    }

    private void OnFuelEmpty()
    {
        isOutOfFuel = true;

        if (kartController != null)
        {
            kartController.canMove = false;
        }

        Debug.Log("Out of fuel! Kart stopped.");
    }
}
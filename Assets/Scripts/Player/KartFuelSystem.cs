using UnityEngine;

public class KartFuelSystem : MonoBehaviour
{
    [Header("Fuel Configuration")]
    public float currentFuel = 100f;
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 10f; // Quantity consumed per second

    [Header("Rewards and Penalties")]
    public float fuelFromBarrel = 25f;      // Recompensa base
    public float fuelLostFromOil = 15f;     // Oil penalty
    

    private bool isOutOfFuel = false;

    private KartLaneController kartController;

    private KartLaneController kartController;
    private CPUController cpuController;

    public bool IsOutOfFuel => isOutOfFuel;

    void Start()
    {
        rb =
       GetComponent<Rigidbody>();


        kartController =
            GetComponent<KartLaneController>();


        cpuController =
            GetComponent<CPUController>();
    }

    void Update()
    {
        if (isOutOfFuel)
            return;

        // Determines whether it is currently this kart's turn.
        bool isMyTurn =
            (
                gameObject.CompareTag("Player") &&
                GameManager.Instance.gameState ==
                GameManager.GameState.PlayerTurn
            )
            ||
            (
                gameObject.CompareTag("CPU") &&
                GameManager.Instance.gameState ==
                GameManager.GameState.CPUTurn
            );


        // Fuel is consumed only during the corresponding turn.
        if (!isMyTurn)
            return;


        // Consumes fuel gradually while the kart is active.
        if (currentFuel > 0f)
        {
            currentFuel -=
                fuelConsumptionRate *
                Time.deltaTime;


            // Evita que el combustible tenga valores menores que cero o mayores que el máximo permitido.
            currentFuel =
                Mathf.Clamp(
                    currentFuel,
                    0f,
                    maxFuel
                );


            // If fuel reaches zero, the turn ends.
            if (currentFuel <= 0f)
            {
                OnFuelEmpty();
            }
        }
    }


    // Returns the current fuel percentage between 0 and 1..

    public float GetFuelNormalized()
    {
        return currentFuel / maxFuel;
    }


    // Adds fuel to the kart.

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


    // Fuel remains due to the oil slick.

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
        // Marks the kart as out of fuel.
        isOutOfFuel =
            true;


        // Physically stops the Rigidbody.
        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;


            rb.angularVelocity =
                Vector3.zero;
        }


        // Blocks player movement.
        if (
            kartController != null
        )
        {
            kartController.canMove =
                false;
        }


        // Blocks CPU movement.
        if (
            cpuController != null
        )
        {
            cpuController.canMove =
                false;
        }


        Debug.Log(
            "¡We've run out of gas! The go-kart comes to a stop."
        );


        // Notify the GameManager who ran out of fuel..
        if (
            GameManager.Instance != null
        )
        {
            if (
                CompareTag("Player")
            )
            {
                GameManager.Instance
                    .PlayerOutOfFuel();
            }
            else if (
                CompareTag("CPU")
            )
            {
                GameManager.Instance
                    .CPUOutOfFuel();
            }
        }
    }
}
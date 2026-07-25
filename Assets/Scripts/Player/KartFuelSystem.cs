using UnityEngine;

public class KartFuelSystem : MonoBehaviour
{
    [Header("Configuración de Combustible")]
    public float currentFuel = 100f;
    public float maxFuel = 100f;
    public float fuelConsumptionRate = 10f; // Cantidad consumida por segundo
    
    [Header("Recompensas y Penalizaciones")]
    public float fuelFromBarrel = 25f;      // Recompensa base
    public float fuelLostFromOil = 15f;     // Penalización por aceite

    private bool isOutOfFuel = false;
    private Rigidbody rb;

    public bool IsOutOfFuel => isOutOfFuel;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isOutOfFuel) return;

        // Consumo constante
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
    /// Suma combustible al kart. Permite un valor personalizado o usa el valor por defecto.
    /// </summary>
    public void AddFuel(float amount = -1f)
    {
        float amountToAdd = (amount < 0f) ? fuelFromBarrel : amount;

        currentFuel = Mathf.Clamp(currentFuel + amountToAdd, 0f, maxFuel);
        
        // Si el kart estaba detenido por falta de gasolina, reanudamos el estado
        if (currentFuel > 0f && isOutOfFuel)
        {
            isOutOfFuel = false;
            Debug.Log("¡Gasolina recargada! Kart reactivado.");
        }
        else
        {
            Debug.Log($"¡Combustible recargado (+{amountToAdd})! Total: {currentFuel}");
        }
    }

    /// <summary>
    /// Resta combustible por la mancha de aceite.
    /// </summary>
    public void ReduceFuelFromOil()
    {
        currentFuel = Mathf.Clamp(currentFuel - fuelLostFromOil, 0f, maxFuel);
        Debug.Log($"¡Pierdes gasolina por el aceite! Total: {currentFuel}");

        if (currentFuel <= 0f && !isOutOfFuel)
        {
            OnFuelEmpty();
        }
    }

    private void OnFuelEmpty()
    {
        isOutOfFuel = true;

        // Frenar inmediatamente la inercia del Rigidbody para evitar que siga rodando
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("¡Se acabó la gasolina! El kart se detiene.");
    }
}
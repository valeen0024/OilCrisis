using UnityEngine;

public class FuelBarrel : MonoBehaviour
{
    [Header("Configuración del Barril")]
    [SerializeField] private float fuelAmount = 25f;

    private void OnTriggerEnter(Collider other)
    {
        // 1. Buscamos el KartFuelSystem en el objeto que choca o en sus componentes padre
        KartFuelSystem fuelSystem = other.GetComponentInParent<KartFuelSystem>();

        // 2. Si lo encuentra, recarga combustible y destruye el barril
        if (fuelSystem != null)
        {
            fuelSystem.AddFuel(fuelAmount);
            Destroy(gameObject);
        }
    }
}
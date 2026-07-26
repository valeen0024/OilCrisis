using UnityEngine;

public class FuelBarrel : MonoBehaviour
{
    [Header("Configuración del Barril")]
    [SerializeField] private float fuelAmount = 25f;

    [Header("Audio")]
    [Tooltip("Clip de sonido al chocar con el barril.")]
    [SerializeField] private AudioClip impactSound;

    [Tooltip("Volumen del efecto de sonido (0.0 a 1.0).")]
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Recargar combustible si el jugador tiene el sistema de gasolina
            if (other.TryGetComponent<KartFuelSystem>(out KartFuelSystem fuelSystem))
            {
                fuelSystem.AddFuel(fuelAmount);
            }

            // 2. Reproducir el sonido en la posición del impacto (no se corta al destruir el barril)
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position, soundVolume);
            }

            // 3. Destruir el barril recolectado
            Destroy(gameObject);
        }
    }
}
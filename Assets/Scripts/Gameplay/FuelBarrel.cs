using Unity.VisualScripting;
using UnityEngine;

public class FuelBarrel : MonoBehaviour
{
    [Header("Configuración del Barril")]
    [SerializeField] private float fuelAmount = 25f;


    [Header("Audio")]
    [Tooltip("Clip de sonido al recoger el barril.")]
    [SerializeField] private AudioClip impactSound;


    [Tooltip("Volumen del efecto de sonido.")]
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    //Detects collision via trigger with other objects.
    private void OnTriggerEnter(Collider other)
    {
        //Checks if the colliding object belongs to the Player or CPU.
        if (
            other.CompareTag("Player")
            ||
            other.CompareTag("CPU")
        )
        {
            //Attempts to get the kart's fuel system component.
            if (
                other.TryGetComponent<KartFuelSystem>(
                    out KartFuelSystem fuelSystem
                )
            )
            {
                //Increases the kart's fuel using the defined amount.
                fuelSystem.AddFuel(
                    fuelAmount
                );
            }


            if (
                //Plays the impact sound at the barrel's position if assigned.
                impactSound
                !=
                null
            )
            {
                AudioSource.PlayClipAtPoint(
                    impactSound,
                    transform.position,
                    soundVolume
                );
            }


            //Deactivates the object to allow subsequent respawning
            gameObject.SetActive(false);
        }
    }
}
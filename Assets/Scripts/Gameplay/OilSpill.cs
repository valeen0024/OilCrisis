using UnityEngine;

public class OilSpill : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip oilSound;

    [Header("Settings")]
    [SerializeField] private bool destroyOnStepped = false; // Por si quieres que la mancha desaparezca al pisarla

    private bool hasBeenStepped = false;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si lo que pisó la mancha es el Jugador o la CPU (usando Tags o un componente)
        if (!hasBeenStepped && (other.CompareTag("Player") || other.CompareTag("CPU")))
        {
            hasBeenStepped = true;
            PlayOilSound();

            if (destroyOnStepped)
            {
                // Esconde el modelo visual y destruye el objeto después de que termine el audio
                GetComponent<MeshRenderer>().enabled = false;
                Destroy(gameObject, oilSound != null ? oilSound.length : 0.5f);
            }
        }
    }

    private void PlayOilSound()
    {
        if (audioSource != null && oilSound != null)
        {
            audioSource.PlayOneShot(oilSound);
        }
    }
}
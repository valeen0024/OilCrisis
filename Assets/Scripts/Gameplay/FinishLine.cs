using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private AudioSource finishAudio;
    private bool hasFinished = false; // Prevents the audio from playing multiple times

    private void Start()
    {
        // Get the AudioSource component attached to the finish line
        finishAudio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object crossing the line has the "Player" tag and has not finished yet
        if (other.CompareTag("Player") && !hasFinished)
        {
            hasFinished = true;
            finishAudio.Play();
            
            // Add any additional logic here (e.g., stop timer, show victory UI)
            Debug.Log("Finish line reached!");
        }
    }
}
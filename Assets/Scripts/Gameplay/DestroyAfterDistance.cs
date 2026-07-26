using UnityEngine;

public class DestroyAfterDistance : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Reference to the player/kart transform.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Distance behind the player at which this object will be destroyed.")]
    [SerializeField] private float destroyDistanceBehind = 15f;

    private void Start()
    {
        // Automatically find the player by tag if not assigned in Inspector
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Check if the obstacle is far enough behind the player's Z position
        if (playerTransform.position.z - transform.position.z > destroyDistanceBehind)
        {
            Destroy(gameObject);
        }
    }
}
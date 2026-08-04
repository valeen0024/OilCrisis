using Unity.VisualScripting;
using UnityEngine;

public class DestroyAfterDistance : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Distance behind the kart at which the object will be destroyed.")]
    [SerializeField] private float destroyDistanceBehind = 15f;

    private void Update()
    {


        //Items are removed from the track when the CPU's turn ends.
        if (GameManager.Instance.gameState == GameManager.GameState.CPUTurn)
        {
            // Find the CPU kart using the Tag
            GameObject cpuKart = GameObject.FindGameObjectWithTag("CPU");

            if (cpuKart != null)
            {
                // If the CPU has already left an obstacle or decoration far behind, it is removed.
                if (cpuKart.transform.position.z - transform.position.z > destroyDistanceBehind)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
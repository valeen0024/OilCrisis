using UnityEngine;

public class OilSlick : MonoBehaviour
{
    public float slowDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        KartMovement kart = other.GetComponentInParent<KartMovement>();

        if (kart != null || other.CompareTag("Player"))
        {
            if (kart != null)
            {
                kart.ApplyOilSlick(slowDuration);
            }

            Debug.Log("¡Aceite pisado!");
        }
    }
}
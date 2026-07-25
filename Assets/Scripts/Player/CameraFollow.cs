using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    public Transform target;          // Arrastra aquí el KartParent

    [Header("Ajustes de Seguimiento")]
    public Vector3 offset = new Vector3(0f, 5.8f, -6.8f); // Offset inicial basado en tu Inspector
    public float smoothSpeed = 0.125f;                    // Suavizado del movimiento

    void LateUpdate()
    {
        if (target == null) return;

        // Calcula la posición deseada
        Vector3 desiredPosition = target.position + offset;

        // Interpola suavemente entre la posición actual de la cámara y la deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        transform.position = smoothedPosition;
    }
}
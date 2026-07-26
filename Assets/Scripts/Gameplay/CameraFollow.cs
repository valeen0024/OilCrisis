using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    [SerializeField] private Transform target; // Arrastra KartParent aquí

    [Header("Configuración de Distancia")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 3.5f, -7f);
    [SerializeField] private float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Convierte el offset local a coordenadas del mundo considerando la rotación del kart
        Vector3 desiredPosition = target.position + (target.rotation * offset);

        // Movimiento fluido
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Mantener la cámara enfocando siempre al kart
        transform.LookAt(target);
    }
}
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    public Transform target;

    [Header("Configuración de Distancia")]
    public Vector3 offset = new Vector3(0f, 3.5f, -7f);
    public float smoothSpeed = 10f;

    // Stores the fixed center of the track based on the start of the game
    private float fixedTrackCenterX;

    void Start()
    {
        if (target != null)
        {
            fixedTrackCenterX = target.position.x;
            SetTarget(target);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Keeps the X fixed at the center of the track, ignoring lateral lane changes.
        Vector3 desiredPosition = new Vector3(
            fixedTrackCenterX + offset.x,
            target.position.y + offset.y,
            target.position.z + offset.z
        );

        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Always look straight ahead, centered on the track.
        Vector3 lookTarget = new Vector3(fixedTrackCenterX + offset.x, target.position.y, target.position.z);
        transform.LookAt(lookTarget);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
        {
            // Immediately teleports the camera behind the new target while keeping the center fixed.
            transform.position = new Vector3(
                fixedTrackCenterX + offset.x,
                target.position.y + offset.y,
                target.position.z + offset.z
            );
        }
    }
}
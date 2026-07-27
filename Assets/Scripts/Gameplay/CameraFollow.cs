using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;

    // Fixed camera values
    private float fixedX;
    private float fixedY;
    private Quaternion fixedRotation;

    // Original distance between the camera and the target
    private float initialZOffset;

    private void Start()
    {
        if (target == null)
            return;

        // Store the original camera transform
        fixedX = transform.position.x;
        fixedY = transform.position.y;
        fixedRotation = transform.rotation;

        // Store the original distance to the target
        initialZOffset = transform.position.z - target.position.z;

        SnapToTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = new Vector3(
            fixedX,
            fixedY,
            target.position.z + initialZOffset
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Keep the original camera rotation
        transform.rotation = fixedRotation;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target == null)
            return;

        Debug.Log("Camera target changed to: " + target.name);

        SnapToTarget();
    }
    private void SnapToTarget()
    {
        transform.position = new Vector3(
            fixedX,
            fixedY,
            target.position.z + initialZOffset
        );

        transform.rotation = fixedRotation;
    }
}
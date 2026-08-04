using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;

    [Header("Dynamic FOV")]
    [SerializeField] private Camera cam;
    [SerializeField] private float minFOV = 45f;
    [SerializeField] private float maxFOV = 60f;
    [SerializeField] private float fovChangeSpeed = 5f;
    [SerializeField] private float maxKartSpeed = 15f;

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

        // Automatically get the Camera component
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

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

        // Keep X and Y fixed while following only the Z position
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

        // Dynamic Field of View based on kart speed
        if (cam != null)
        {
            KartLaneController laneController = target.GetComponent<KartLaneController>();

            if (laneController != null)
            {
                float speedRatio = Mathf.Clamp01(
                    laneController.currentForwardSpeed / maxKartSpeed
                );

                float targetFOV = Mathf.Lerp(
                    minFOV,
                    maxFOV,
                    speedRatio
                );

                cam.fieldOfView = Mathf.Lerp(
                    cam.fieldOfView,
                    targetFOV,
                    fovChangeSpeed * Time.deltaTime
                );
            }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target == null)
            return;

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
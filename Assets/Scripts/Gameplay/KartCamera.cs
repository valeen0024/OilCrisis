using UnityEngine;

public class KartCamera : MonoBehaviour
{
    [Header("Target References")]
    [SerializeField] private Transform kartTarget;
    [SerializeField] private KartLaneController kartController;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -5f);
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Dynamic FOV Settings")]
    [SerializeField] private Camera cam;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 80f;
    [SerializeField] private float fovChangeSpeed = 3f;

    private void Start()
    {
        // Automatically fetch Camera component if unassigned
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }
    }

    private void LateUpdate()
    {
        if (kartTarget == null) return;

        // 1. Smooth position tracking using Lerp
        Vector3 desiredPosition = kartTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 2. Adjust Camera FOV dynamically based on current kart speed
        if (kartController != null && cam != null)
        {
            // Normalize speed value between 0.0 and 1.0
            float speedRatio = kartController.currentForwardSpeed / 15f; 
            float targetFOV = Mathf.Lerp(minFOV, maxFOV, speedRatio);

            // Interpolate FOV smoothly
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
        }
    }
}
using System.Collections;
using UnityEngine;

public class KartLaneController : MonoBehaviour
{
    [Header("Lane Configuration")]
    [Tooltip("Horizontal distance between each lane center.")]
    [SerializeField] private float laneOffset = 2f;
    
    [Tooltip("Speed at which the kart transitions horizontally between lanes.")]
    [SerializeField] private float laneChangeSpeed = 15f;
    
    [Tooltip("Base forward speed of the kart.")]
    [SerializeField] private float baseForwardSpeed = 6f;

    [Header("Lane Limits (4 Lanes Setup)")]
    [Tooltip("Minimum lane index (most left lane).")]
    [SerializeField] private int minLane = -1;
    
    [Tooltip("Maximum lane index (most right lane).")]
    [SerializeField] private int maxLane = 2;

    [Header("Start Control")]
    [Tooltip("Toggles whether the kart is allowed to respond to input and move.")]
    public bool canMove = true;

    [Header("Current State")]
    [Tooltip("Current lane index (-1, 0, 1, 2).")]
    [SerializeField] private int currentLane = 0; 

    private float currentForwardSpeed;
    private Vector3 targetPosition;
    private KartFuelSystem fuelSystem;
    private bool isSliding = false;

    // Stores the initial X coordinate of the kart set in the Scene
    private float startXPosition; 

    private void Start()
    {
        fuelSystem = GetComponent<KartFuelSystem>();
        currentForwardSpeed = baseForwardSpeed;
        targetPosition = transform.position;

        // Capture the initial X position as the origin for the reference lane (0)
        startXPosition = transform.position.x;
    }

    private void Update()
    {
        if (!canMove) return;

        HandleLaneInput();
        UpdateSpeedBasedOnFuel();
        MoveKart();
    }

    /// <summary>
    /// Processes player horizontal movement input for 4 lanes.
    /// </summary>
    private void HandleLaneInput()
    {
        if (fuelSystem != null && fuelSystem.IsOutOfFuel) return;

        // Move Left
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentLane > minLane) currentLane--;
        }
        // Move Right (allows reaching the 4th lane at index 2)
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentLane < maxLane) currentLane++;
        }
    }

    /// <summary>
    /// Decelerates the kart smoothly to a complete stop if it runs out of fuel.
    /// </summary>
    private void UpdateSpeedBasedOnFuel()
    {
        if (fuelSystem != null && fuelSystem.IsOutOfFuel)
        {
            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, 0f, Time.deltaTime * 3f);
        }
    }

    /// <summary>
    /// Handles forward movement and lane changes using MoveTowards to guarantee precision.
    /// </summary>
    private void MoveKart()
    {
        // Calculate target X position based on the offset and current lane index
        float targetX = startXPosition + (currentLane * laneOffset);
        
        Vector3 currentPos = transform.position;

        // MoveTowards snaps perfectly to targetX without floating-point offset drift
        float newX = Mathf.MoveTowards(currentPos.x, targetX, laneChangeSpeed * Time.deltaTime);
        
        // Continuous forward progress
        float newZ = currentPos.z + (currentForwardSpeed * Time.deltaTime);

        // Apply calculated position
        transform.position = new Vector3(newX, currentPos.y, newZ);
    }

    // ==========================================
    // OIL / OBSTACLE INTERACTION
    // ==========================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Oil"))
        {
            if (fuelSystem != null)
            {
                fuelSystem.ReduceFuelFromOil();
            }

            float duration = 2f;
            if (other.TryGetComponent<OilSlick>(out OilSlick oilScript))
            {
                duration = oilScript.slowDuration;
            }

            ApplyOilSlow(duration);
        }
    }

    /// <summary>
    /// Triggers the temporary slow-down effect caused by oil slicks.
    /// </summary>
    public void ApplyOilSlow(float duration)
    {
        if (!isSliding)
        {
            StartCoroutine(OilSlowRoutine(duration));
        }
    }

    /// <summary>
    /// Coroutine managing speed reduction and recovery duration when slipping on oil.
    /// </summary>
    private IEnumerator OilSlowRoutine(float duration)
    {
        isSliding = true;
        currentForwardSpeed = baseForwardSpeed * 0.5f;
        Debug.Log("Slipped on oil! Speed reduced.");

        yield return new WaitForSeconds(duration);

        if (fuelSystem == null || !fuelSystem.IsOutOfFuel)
        {
            currentForwardSpeed = baseForwardSpeed;
        }

        isSliding = false;
        Debug.Log("Normal speed recovered.");
    }
}
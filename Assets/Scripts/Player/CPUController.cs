using System.Collections;
using UnityEngine;

public class CPUController : MonoBehaviour
{
    [Header("Lane Configuration")]
    [Tooltip("Horizontal distance between each lane center.")]
    [SerializeField] private float laneOffset = 2f;

    [Tooltip("Speed at which the kart transitions horizontally between lanes.")]
    [SerializeField] private float laneChangeSpeed = 15f;

    [Tooltip("Forward speed of the CPU kart.")]
    [SerializeField] private float cpuSpeed = 6f;

    [Header("Lane Limits (4 Lanes Setup)")]
    [Tooltip("Minimum lane index (leftmost lane).")]
    [SerializeField] private int minLane = -2;

    [Tooltip("Maximum lane index (rightmost lane).")]
    [SerializeField] private int maxLane = 1;

    [Header("AI Control")]
    [Tooltip("Allows the CPU kart to move.")]
    public bool canMove = false;

    [Tooltip("How often the CPU chooses a new lane.")]
    [SerializeField] private float laneChangeInterval = 2f;

    [Tooltip("How far ahead the CPU scans for objects.")]
    [SerializeField] private float scanDistance = 20f;

    [Header("Lane Weights")]
    [Tooltip("Weight for an empty lane.")]
    [SerializeField] private float emptyLaneWeight = 10f;

    [Tooltip("Weight for a lane with a fuel can.")]
    [SerializeField] private float fuelCanWeight = 50f;

    [Tooltip("Weight for a lane with oil.")]
    [SerializeField] private float oilSpillWeight = 2f;

    [Header("Oil Effect")]
    [SerializeField] private float oilSlowMultiplier = 0.5f;
    [SerializeField] private float oilSlowDuration = 2f;

    [Header("Start Lane")]
    [Tooltip("Initial lane of the CPU kart. Example: -1, 0, 1, 2.")]
    [SerializeField] private int startLane = 1;

    [Header("Turbo / Boost Settings")]
    [Tooltip("Speed multiplier during the boost.")]
    [SerializeField] private float boostSpeedMultiplier = 2f;
    [Tooltip("Duration of the boost in seconds.")]
    [SerializeField] private float boostDuration = 2f;
    [Tooltip("Chance per second (0.0 to 1.0) for the CPU to randomly activate the boost.")]
    [SerializeField] private float boostChancePerSecond = 0.2f;

    private bool hasUsedTurbo = false; // Single use control
    private Animator animator;

    private int currentLane;
    private int targetLane;
    private float laneChangeTimer;
    private float startXPosition;

    private float currentForwardSpeed;
    private KartFuelSystem fuelSystem;
    private bool isSliding = false;

    // Initial transform
    private Vector3 startPosition;
    private Quaternion startRotation;
    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation; 

        fuelSystem = GetComponent<KartFuelSystem>();

        animator = GetComponentInChildren<Animator>();

        // Save the X position where this kart starts.
        startXPosition = transform.position.x;

        // Start in the assigned lane.
        currentLane = Mathf.Clamp(startLane, minLane, maxLane);
        targetLane = currentLane;

        // Set the initial forward speed.
        currentForwardSpeed = cpuSpeed;

        // Start the lane decision timer.
        laneChangeTimer = laneChangeInterval;
    }

    private void Update()
    {
        // If movement is blocked, do nothing.
        if (!canMove)
            return;

        // If the GameManager does not exist, do nothing.
        if (GameManager.Instance == null)
            return;

        // The CPU only moves during its own turn.
        if (GameManager.Instance.gameState != GameManager.GameState.CPUTurn)
            return;

        // If the kart has no fuel, it cannot move.
        if (fuelSystem != null && fuelSystem.IsOutOfFuel)
            return;

        MoveCPU();

        //Randomly evaluate if the CPU should use its single-use turbo
        if (!hasUsedTurbo)
        {
            // Generates a random value. Scales with deltaTime so the chance is per second.
            if (Random.value < boostChancePerSecond * Time.deltaTime)
            {
                ActivateTurbo();
            }
        }

        laneChangeTimer -= Time.deltaTime;
        if (laneChangeTimer <= 0f)
        {
            ChooseLane();
            laneChangeTimer = laneChangeInterval;
        }
    }

    // Moves the kart forward and smoothly to the target lane.
    private void MoveCPU()
    {
        float targetX = GetLanePosition(targetLane);

        Vector3 currentPos = transform.position;

        // Smooth horizontal lane change.
        float newX = Mathf.MoveTowards(
            currentPos.x,
            targetX,
            laneChangeSpeed * Time.deltaTime
        );

        // Forward movement, same style as the player's kart.
        float newZ = currentPos.z + (currentForwardSpeed * Time.deltaTime);

        // Prevent leaving the track on the sides.
        float minX = GetLanePosition(minLane);
        float maxX = GetLanePosition(maxLane);
        newX = Mathf.Clamp(newX, minX, maxX);

        transform.position = new Vector3(newX, currentPos.y, newZ);

        // When it reaches the lane, update the current lane.
        if (Mathf.Abs(transform.position.x - targetX) < 0.05f)
        {
            currentLane = targetLane;
        }
    }

    // Chooses a new lane using weights based on nearby objects.
    private void ChooseLane()
    {
        // It can only move one lane to the left or to the right.
        int[] possibleLanes;

        if (currentLane <= minLane)
        {
            possibleLanes = new int[] { currentLane + 1 };
        }
        else if (currentLane >= maxLane)
        {
            possibleLanes = new int[] { currentLane - 1 };
        }
        else
        {
            possibleLanes = new int[] { currentLane - 1, currentLane + 1 };
        }

        // Choose one of the adjacent lanes.
        targetLane = possibleLanes[Random.Range(0, possibleLanes.Length)];
    }

    // Returns a weight for a lane based on what the CPU detects ahead.
    private float EvaluateLaneWeight(int lane)
    {
        float targetX = GetLanePosition(lane);

        Vector3 rayOrigin = new Vector3(
            targetX,
            transform.position.y + 0.5f,
            transform.position.z
        );

        if (Physics.Raycast(rayOrigin, Vector3.forward, out RaycastHit hit, scanDistance))
        {
            if (hit.collider.CompareTag("FuelCan"))
                return fuelCanWeight;

            if (hit.collider.CompareTag("OilSpill"))
                return oilSpillWeight;
        }

        return emptyLaneWeight;
    }

    // Picks one lane based on the weights array.
    private int GetWeightedRandomLane(float[] weights)
    {
        float totalWeight = 0f;

        for (int i = 0; i < weights.Length; i++)
            totalWeight += weights[i];

        if (totalWeight <= 0f)
        {
            // Fallback: choose any lane except the current one.
            int fallbackLane = currentLane;
            while (fallbackLane == currentLane)
            {
                fallbackLane = Random.Range(minLane, maxLane + 1);
            }
            return fallbackLane - minLane;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
                return i;
        }

        return 0;
    }

    // Gets the X position for a given lane.
    private float GetLanePosition(int lane)
    {
        return startXPosition + (lane * laneOffset);
    }

    private void OnTriggerEnter(Collider other)
    {
       
        Debug.Log(
            "The CPU came into contact with: "
            + other.gameObject.name
            + " | Tag: "
            + other.gameObject.tag
        );


        // Comprueba si el objeto es un barril de combustible.
        if (other.CompareTag("Fuel"))
        {
            Debug.Log(
                "The CPU picked up a fuel barrel."
            );


            // Checks if the object is a fuel barrel..
            if (fuelSystem != null)
            {
                Debug.Log(
                    "Increasing fuel."
                );

                fuelSystem.AddFuel();
            }



            // Deactivates the barrel after picking it up.
            other.gameObject.SetActive(false);


            Debug.Log(
                "Barrel deactivated."
            );


            return;
        }


        // Checks if the object is an oil slick.
        if (other.CompareTag("Oil"))
        {
            Debug.Log(
                "The CPU stepped on an oil slick."
            );


            // Reduces fuel consumption.
            if (fuelSystem != null)
            {
                Debug.Log(
                    "Reducing fuel in favor of oil."
                );

                fuelSystem.ReduceFuelFromOil();
            }



            // Applies the speed reduction.
            ApplyOilSlow(
                oilSlowDuration
            );


            Debug.Log(
                "Effect of applied oil."
            );
        }
    }

    // Applies a temporary speed reduction.
    private void ApplyOilSlow(float duration)
    {
        if (!isSliding)
        {
            StartCoroutine(OilSlowRoutine(duration));
        }
    }

    private IEnumerator OilSlowRoutine(float duration)
    {
        isSliding = true;

        float originalSpeed = currentForwardSpeed;
        currentForwardSpeed = cpuSpeed * oilSlowMultiplier;

        Debug.Log("CPU slipped on oil. Speed reduced.");

        yield return new WaitForSeconds(duration);

        if (fuelSystem == null || !fuelSystem.IsOutOfFuel)
        {
            currentForwardSpeed = cpuSpeed;
        }

        isSliding = false;
        Debug.Log("CPU speed recovered.");
    }
    public void ResetForNewTurn()
    {
        StopAllCoroutines();

        // Disable movement until the traffic light enables it again
        canMove = false;

        // Restore the original position and rotation
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Reset lane information
        currentLane = Mathf.Clamp(startLane, minLane, maxLane);
        targetLane = currentLane;

        // Reset movement values
        currentForwardSpeed = cpuSpeed;
        laneChangeTimer = laneChangeInterval;

        // Clear oil effect
        isSliding = false;

        hasUsedTurbo = false;

        // Restore fuel
        if (fuelSystem != null)
        {
            fuelSystem.currentFuel = fuelSystem.maxFuel;
        }
    }

    private void ActivateTurbo()
    {
        hasUsedTurbo = true; // Blocks future uses

        // Calls the animation trigger
        if (animator != null)
        {
            animator.SetTrigger("Boost");
        }

        StartCoroutine(TurboRoutine());
    }

    private IEnumerator TurboRoutine()
    {
        currentForwardSpeed = cpuSpeed * boostSpeedMultiplier;
        Debug.Log("CPU activated Boost randomly!");

        yield return new WaitForSeconds(boostDuration);

        // Restores speed only if not out of fuel
        if (fuelSystem == null || !fuelSystem.IsOutOfFuel)
        {
            currentForwardSpeed = cpuSpeed;
        }

        Debug.Log("CPU Boost finished. Normal speed restored.");
    }

  
}
using System;
using System.Collections;
using UnityEngine;

public class KartLaneController : MonoBehaviour
{
    [Header("Lane Configuration")]
    [Tooltip("Horizontal distance between each lane center.")]
    [SerializeField] private float laneOffset = 2f;
    
    [Tooltip("Speed at which the kart transitions horizontally between lanes.")]
    [SerializeField] private float laneChangeSpeed = 15f;

    [Header("Speed & Inertia Settings")]
    [Tooltip("Target max forward speed during normal driving.")]
    [SerializeField] private float baseForwardSpeed = 15f;

    [Tooltip("How fast the vehicle accelerates up to its target speed.")]
    [SerializeField] private float acceleration = 12f;

    [Tooltip("How fast the vehicle loses speed when releasing gas or idling.")]
    [SerializeField] private float friction = 2f;

    [Header("Lane Limits (4 Lanes Setup)")]
    [Tooltip("Minimum lane index (most left lane).")]
    [SerializeField] private int minLane = -1;
    
    [Tooltip("Maximum lane index (most right lane).")]
    [SerializeField] private int maxLane = 2;

    [Header("Start Control")]
    [Tooltip("Toggles whether the kart is allowed to respond to input and move.")]
    public bool canMove = false;

    [Header("Current State")]
    [Tooltip("Current lane index (-1, 0, 1, 2).")]
    [SerializeField] private int currentLane = 0;

    [Header("Turbo / Boost Settings")]
    [Tooltip("Speed multiplier during the boost.")]
    [SerializeField] private float boostSpeedMultiplier = 1.8f;

    [Tooltip("Duration of the boost in seconds.")]
    [SerializeField] private float boostDuration = 2f;

    [Header("Audio Settings")]
    [Tooltip("Rngine sound.")]
    [SerializeField] private AudioClip engineClip;
    [Tooltip("Turbo sound.")]
    [SerializeField] private AudioClip turboClip;

    [Tooltip("Normal engine volume and tone.")]
    [SerializeField] private float normalEngineVolume = 0.5f;
    [SerializeField] private float normalEnginePitch = 1.0f;

    [Tooltip("VEngine volume and tone when the turbo is active.")]
    [SerializeField] private float turboEngineVolume = 1.0f;
    [SerializeField] private float turboEnginePitch = 1.5f;

    [Tooltip("Duration of the Fade In / Fade Out effect in seconds.")]
    [SerializeField] private float engineFadeDuration = 1.0f;

    [Tooltip("Duration in seconds for the engine sound to return to normal after the turbo.")]
    [SerializeField] private float turboCooldownDuration = 0.5f;

    private bool isFadingOut = false;


    private AudioSource engineAudioSource;
    private AudioSource turboAudioSource;

    private bool hasUsedTurbo = false; // Ensures it is a single-use boost
    private Animator animator;         // Reference to trigger the animation

    public float currentForwardSpeed;  // Current actual speed (interpolated)
    private float targetForwardSpeed;   // Desired speed calculated by state & input
    private Vector3 targetPosition;
    private KartFuelSystem fuelSystem;
    private bool isSliding = false;
    private bool isBoosting = false;

    // Stores the initial X coordinate of the kart set in the Scene
    private float startXPosition; 

    private void Start()
    {
        fuelSystem = GetComponent<KartFuelSystem>();

        // Finds the Animator on this object or its children.
        animator = GetComponentInChildren<Animator>();
        
        targetForwardSpeed = 0f;
        currentForwardSpeed = 0f;
        targetPosition = transform.position;

        // Capture the initial X position as the origin for the reference lane (0)
        startXPosition = transform.position.x;


        //Create the Engine AudioSource
        if (engineClip != null)
        {
            engineAudioSource = gameObject.AddComponent<AudioSource>();
            engineAudioSource.clip = engineClip;
            engineAudioSource.loop = true;
            engineAudioSource.volume = normalEngineVolume;
            engineAudioSource.pitch = normalEnginePitch;
            engineAudioSource.playOnAwake = false;
        }

        //Create the Turbo AudioSource
        if (turboClip != null)
        {
            turboAudioSource = gameObject.AddComponent<AudioSource>();
            turboAudioSource.clip = turboClip;
            turboAudioSource.loop = false; 
            turboAudioSource.playOnAwake = false;
        }
    }



    private void Update()
    {
        if (!canMove) return;

        // Detect single-use boost input (Key Code 'W' or 'Up Arrow')
        if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && !hasUsedTurbo)
        {
            ActivateTurbo();
        }

        HandleLaneInput();
        CalculateTargetSpeed();
        ApplyInertiaAndSpeed();
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
    /// Determines the desired target speed based on fuel, oil slicks, and turbo state.
    /// </summary>
    private void CalculateTargetSpeed()
    {
        // 1. Out of fuel -> Stop completely
        if (fuelSystem != null && fuelSystem.IsOutOfFuel)
        {
            targetForwardSpeed = 0f;
            return;
        }

        // 2. Currently in Boost state
        if (isBoosting)
        {
            targetForwardSpeed = baseForwardSpeed * boostSpeedMultiplier;
            return;
        }

        // 3. Currently slipping on Oil
        if (isSliding)
        {
            targetForwardSpeed = baseForwardSpeed * 0.5f;
            return;
        }

        // 4. Normal movement target
        targetForwardSpeed = baseForwardSpeed;
    }

    /// <summary>
    /// Smoothly transitions currentForwardSpeed toward targetForwardSpeed using acceleration and friction.
    /// </summary>
    private void ApplyInertiaAndSpeed()
    {
        if (currentForwardSpeed < targetForwardSpeed)
        {
            // Accelerate smoothly toward the target speed
            currentForwardSpeed += acceleration * Time.deltaTime;
            currentForwardSpeed = Mathf.Min(currentForwardSpeed, targetForwardSpeed);
        }
        else if (currentForwardSpeed > targetForwardSpeed)
        {
            // Decelerate/apply friction when slowing down or running out of fuel
            currentForwardSpeed -= friction * 5f * Time.deltaTime;
            currentForwardSpeed = Mathf.Max(currentForwardSpeed, targetForwardSpeed);
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
        
        // Continuous forward progress using smoothed inertia speed
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

    // ==========================================
    // TURBO / BOOST SYSTEM
    // ==========================================
    private void ActivateTurbo()
    {
        hasUsedTurbo = true; // Blocks future uses (Single use)

        // Calls the exact "Boost" trigger defined in KartAnimator.controller
        if (animator != null)
        {
            animator.SetTrigger("Boost");
        }
        //Activate the turbo sound.
        if (turboAudioSource != null)
        {
            turboAudioSource.Play();
        }

        // Starts the speed increase
        StartCoroutine(TurboRoutine());
    }

    public void StartEngineSound()
    {
        if (engineAudioSource != null && !engineAudioSource.isPlaying)
        {
            StartCoroutine(FadeInEngine());
        }
    }

    //Fade-in effect at the start of the engine sound.
    private IEnumerator FadeInEngine()
    {
        
        engineAudioSource.volume = 0f;
       
        engineAudioSource.Play();

        float currentTime = 0f;

        
        while (currentTime < engineFadeDuration)
        {
            currentTime += Time.deltaTime;
          
            engineAudioSource.volume = Mathf.Lerp(0f, normalEngineVolume, currentTime / engineFadeDuration);
            yield return null; // Esperamos al siguiente frame
        }

      
        engineAudioSource.volume = normalEngineVolume;
    }
    //Stops the engine sound
    public void StopEngineSound()
    {
        
        if (engineAudioSource != null && engineAudioSource.isPlaying && !isFadingOut)
        {
            StartCoroutine(FadeOutEngine());
        }
    }

    // Gradual fade out of the engine sound when the kart stops.
    private IEnumerator FadeOutEngine()
    {
        isFadingOut = true;

        
        float startVolume = engineAudioSource.volume;
        float currentTime = 0f;

        while (currentTime < engineFadeDuration)
        {
            currentTime += Time.deltaTime;
            engineAudioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / engineFadeDuration);
            yield return null;
        }

        
        engineAudioSource.volume = 0f;
        engineAudioSource.Stop();
        isFadingOut = false;
    }

    private IEnumerator TurboRoutine()
    {
        isBoosting = true;
        Debug.Log("Boost activated!");

        //Increases the volume and intensifies the sound of the engine when the turbo is activated.
        if (engineAudioSource != null)
        {
            engineAudioSource.volume = turboEngineVolume;
            engineAudioSource.pitch = turboEnginePitch;
        }

        // Waits for the assigned duration
        yield return new WaitForSeconds(boostDuration);

        // Once finished, restores the original speed 
        // (Making sure it hasn't run out of fuel in the meantime)
        if (fuelSystem == null || !fuelSystem.IsOutOfFuel)
        {
            currentForwardSpeed = baseForwardSpeed;
        }

        //When the turbo stops, the engine sound returns to normal.
        if (engineAudioSource != null)
        {
            float startVolume = engineAudioSource.volume;
            float startPitch = engineAudioSource.pitch;
            float currentTime = 0f;

            
            while (currentTime < turboCooldownDuration)
            {
                currentTime += Time.deltaTime;

                engineAudioSource.volume = Mathf.Lerp(startVolume, normalEngineVolume, currentTime / turboCooldownDuration);
                engineAudioSource.pitch = Mathf.Lerp(startPitch, normalEnginePitch, currentTime / turboCooldownDuration);

                yield return null; 
            }

            
            engineAudioSource.volume = normalEngineVolume;
            engineAudioSource.pitch = normalEnginePitch;

            
        }

        isBoosting = false;
        Debug.Log("Boost finished. Normal speed restored.");
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
        Debug.Log("Slipped on oil! Speed reduced.");

        yield return new WaitForSeconds(duration);

        isSliding = false;
        Debug.Log("Normal speed recovered.");
    }
}
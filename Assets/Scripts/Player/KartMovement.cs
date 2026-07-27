using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class KartMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 70f;
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField] private float maxSpeed = 35f;

    private Rigidbody rb;
    private KartFuelSystem fuelSystem;

    private float moveInput;
    private float turnInput;

    // Oil effect state
    private bool isSliding = false;
    private float baseSpeed;
    private float baseMaxSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        fuelSystem = GetComponent<KartFuelSystem>();

        // Lower the center of mass to improve stability while turning
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);

        // Store the original movement values
        baseSpeed = speed;
        baseMaxSpeed = maxSpeed;
    }

    private void Update()
    {
        // Stop reading input if the kart is out of fuel
        if (fuelSystem != null && fuelSystem.IsOutOfFuel)
        {
            moveInput = 0f;
            turnInput = 0f;
            return;
        }

        // Read keyboard input using the Input System
        if (Keyboard.current != null)
        {
            moveInput = 0f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                moveInput += 1f;

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                moveInput -= 1f;

            turnInput = 0f;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                turnInput += 1f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                turnInput -= 1f;
        }
    }

    private void FixedUpdate()
    {
        // Ignore physics updates while the Rigidbody is kinematic
        if (rb.isKinematic)
            return;

        // Gradually stop the kart when fuel runs out
        if (fuelSystem != null && fuelSystem.IsOutOfFuel)
        {
            rb.linearVelocity = Vector3.MoveTowards(
                rb.linearVelocity,
                Vector3.zero,
                Time.fixedDeltaTime * 10f);

            return;
        }

        HandleMovement();
        HandleSteering();
    }

    private void HandleMovement()
    {
        float currentForwardSpeed = transform.InverseTransformDirection(rb.linearVelocity).z;

        // Forward acceleration
        if (moveInput > 0f && currentForwardSpeed < maxSpeed)
        {
            rb.AddForce(transform.forward * moveInput * speed, ForceMode.Acceleration);
        }
        // Reverse movement
        else if (moveInput < 0f && currentForwardSpeed > -maxSpeed / 2f)
        {
            rb.AddForce(transform.forward * moveInput * speed, ForceMode.Acceleration);
        }
    }

    private void HandleSteering()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float currentForwardSpeed = transform.InverseTransformDirection(rb.linearVelocity).z;

            // Reverse steering direction while driving backwards
            float directionModifier = currentForwardSpeed < -0.1f ? -1f : 1f;

            float turn = turnInput * turnSpeed * directionModifier * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    // =====================================================
    // Collision Detection
    // =====================================================

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

            ApplyOilSlick(duration);
        }
    }

    // =====================================================
    // Oil Slick Effect
    // =====================================================

    public void ApplyOilSlick(float duration)
    {
        if (isSliding)
        {
            StopCoroutine(nameof(OilEffectRoutine));
        }

        StartCoroutine(OilEffectRoutine(duration));
    }

    private IEnumerator OilEffectRoutine(float duration)
    {
        isSliding = true;

        speed = baseSpeed * 0.5f;
        maxSpeed = baseMaxSpeed * 0.5f;

        Debug.Log("Oil slick detected. Speed reduced.");

        yield return new WaitForSeconds(duration);

        speed = baseSpeed;
        maxSpeed = baseMaxSpeed;

        isSliding = false;

        Debug.Log("Movement restored.");
    }
}
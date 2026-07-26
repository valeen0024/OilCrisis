using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class KartMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float speed = 35f;
    [SerializeField] private float turnSpeed = 80f;
    [SerializeField] private float maxSpeed = 20f;

    private Rigidbody rb;
    private KartFuelSystem fuelSystem;

    private float moveInput;
    private float turnInput;
    
    // Control para el estado de derrape / aceite
    private bool isSliding = false;
    private float baseSpeed;
    private float baseMaxSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fuelSystem = GetComponent<KartFuelSystem>();

        // Bajar el centro de masa previene que el kart se vuelque en curvas cerradas
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);

        // Guardamos las estadísticas base al arrancar
        baseSpeed = speed;
        baseMaxSpeed = maxSpeed;
    }

    void Update()
    {
        // Si no hay gasolina, reseteamos el input
        if (fuelSystem != null && fuelSystem.IsOutOfFuel)
        {
            moveInput = 0f;
            turnInput = 0f;
            return;
        }

        // Lectura usando el Input System
        if (Keyboard.current != null)
        {
            moveInput = 0f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput -= 1f;

            turnInput = 0f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) turnInput += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) turnInput -= 1f;
        }
    }

    void FixedUpdate()
    {
        // Si se acabó el combustible, forzamos la desaceleración inmediata
        if (fuelSystem != null && fuelSystem.IsOutOfFuel) 
        {
            rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 10f);
            return;
        }

        HandleMovement();
        HandleSteering();
    }

    private void HandleMovement()
    {
        float currentForwardSpeed = transform.InverseTransformDirection(rb.linearVelocity).z;

        // Aceleración hacia adelante
        if (moveInput > 0f && currentForwardSpeed < maxSpeed)
        {
            rb.AddForce(transform.forward * moveInput * speed, ForceMode.Force);
        }
        // Reversa
        else if (moveInput < 0f && currentForwardSpeed > -maxSpeed / 2f)
        {
            rb.AddForce(transform.forward * moveInput * speed, ForceMode.Force);
        }
    }

    private void HandleSteering()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float currentForwardSpeed = transform.InverseTransformDirection(rb.linearVelocity).z;
            
            // Inversión limpia de la dirección de giro si va en reversa
            float directionModifier = currentForwardSpeed < -0.1f ? -1f : 1f;
            
            float turn = turnInput * turnSpeed * directionModifier * Time.fixedDeltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            
            rb.MoveRotation(rb.rotation * turnRotation);
        }
    }

    // ==========================================
    // DETECCIÓN DE COLISIONES / TRIGGERS
    // ==========================================

    private void OnTriggerEnter(Collider other)
    {
        // Pisar Mancha de Aceite
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

    // ==========================================
    // SISTEMA DE EFECTO DE ACEITE (OilSlick)
    // ==========================================

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
        Debug.Log("¡Pisaste aceite! Velocidad reducida.");

        yield return new WaitForSeconds(duration);

        speed = baseSpeed;
        maxSpeed = baseMaxSpeed;
        isSliding = false;
        Debug.Log("Control recuperado.");
    }
}
using System.Collections;
using UnityEngine;

public class KartLaneController : MonoBehaviour
{
    [Header("Configuración de Carriles")]
    [SerializeField] private float laneOffset = 3f;
    [SerializeField] private float laneChangeSpeed = 10f;
    [SerializeField] private float baseForwardSpeed = 6f;

    [Header("Control de Inicio")]
    public bool canMove = true;

    [Header("Estado Actual")]
    [SerializeField] private int currentLane = 0; // -1: Izquierda, 0: Centro, 1: Derecha

    private float currentForwardSpeed;
    private Vector3 targetPosition;
    private KartFuelSystem fuelSystem;
    private bool isSliding = false;

    void Start()
    {
        fuelSystem = GetComponent<KartFuelSystem>();
        currentForwardSpeed = baseForwardSpeed;
        targetPosition = transform.position;
    }

    void Update()
    {
        if (!canMove) return;

        HandleLaneInput();
        UpdateSpeedBasedOnFuel();
        MoveKart();
    }

    private void HandleLaneInput()
    {
        // Cambio de carril con A/D o Flechas (Solo si tiene gasolina)
        if (fuelSystem != null && fuelSystem.IsOutOfFuel) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentLane > -1) currentLane--;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentLane < 1) currentLane++;
        }
    }

    private void UpdateSpeedBasedOnFuel()
    {
        // Si se quedó sin gasolina, desacelera suavemente hasta llegar a 0
        if (fuelSystem != null && fuelSystem.IsOutOfFuel)
        {
            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, 0f, Time.deltaTime * 3f);
        }
    }

    private void MoveKart()
    {
        // Calculamos la posición objetivo del carril en X
        float targetX = currentLane * laneOffset;
        
        // Mantenemos Z avanzando con la velocidad actual
        Vector3 currentPos = transform.position;
        float newX = Mathf.Lerp(currentPos.x, targetX, Time.deltaTime * laneChangeSpeed);
        float newZ = currentPos.z + (currentForwardSpeed * Time.deltaTime);

        transform.position = new Vector3(newX, currentPos.y, newZ);
    }

    // ==========================================
    // INTERACCIÓN CON ACEITE / BARRIL
    // ==========================================

    private void OnTriggerEnter(Collider other)
    {
        // Pisar Aceite
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

    public void ApplyOilSlow(float duration)
    {
        if (!isSliding)
        {
            StartCoroutine(OilSlowRoutine(duration));
        }
    }

    private IEnumerator OilSlowRoutine(float duration)
    {
        isSliding = true;
        
        // Reduce la velocidad a la mitad mientras esté bajo efecto del aceite
        currentForwardSpeed = baseForwardSpeed * 0.5f;
        Debug.Log("¡Pisaste aceite! Velocidad reducida.");

        yield return new WaitForSeconds(duration);

        // Solo restaura la velocidad base si aún le queda gasolina
        if (fuelSystem == null || !fuelSystem.IsOutOfFuel)
        {
            currentForwardSpeed = baseForwardSpeed;
        }

        isSliding = false;
        Debug.Log("Velocidad normal recuperada.");
    }
}
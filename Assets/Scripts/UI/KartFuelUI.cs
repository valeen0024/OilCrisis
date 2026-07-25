using UnityEngine;
using UnityEngine.UI;

public class KartFuelUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private KartFuelSystem fuelSystem;
    [SerializeField] private Slider fuelSlider;

    [Header("Configuración Visual")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullFuelColor = Color.green;
    [SerializeField] private Color lowFuelColor = Color.red;

    void Start()
    {
        // Si no se asignaron en el inspector, intenta encontrarlos automáticamente
        if (fuelSystem == null)
        {
            fuelSystem = FindFirstObjectByType<KartFuelSystem>();
        }

        if (fuelSlider == null)
        {
            fuelSlider = GetComponent<Slider>();
        }

        if (fuelSystem != null && fuelSlider != null)
        {
            fuelSlider.maxValue = fuelSystem.maxFuel;
            fuelSlider.value = fuelSystem.currentFuel;
        }
    }

    void Update()
    {
        if (fuelSystem == null || fuelSlider == null) return;

        // Actualiza el valor del slider suavemente
        fuelSlider.value = Mathf.Lerp(fuelSlider.value, fuelSystem.currentFuel, Time.deltaTime * 10f);

        // Opcional: Cambia el color de la barra de verde a rojo cuando le queda poco combustible
        if (fillImage != null)
        {
            float fuelPercent = fuelSystem.currentFuel / fuelSystem.maxFuel;
            fillImage.color = Color.Lerp(lowFuelColor, fullFuelColor, fuelPercent);
        }
    }
}
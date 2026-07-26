using System.Collections;
using UnityEngine;

public class TrafficLightManager : MonoBehaviour
{
    [Header("Referencias del Kart")]
    public KartLaneController kartController;

    [Header("Referencias de los Semáforos en Escena")]
    // Arrastra aquí los objetos "semaforo" y "semaforo (1)" de tu Jerarquía
    public SpriteRenderer[] trafficLightRenderers; 

    [Header("Sprites de las Luces (PNGs)")]
    public Sprite allOffSprite;    // Imagen con todo apagado
    public Sprite redLightSprite;  // Imagen con luz roja encendida
    public Sprite yellowLightSprite; // Imagen con luz amarilla encendida
    public Sprite greenLightSprite; // Imagen con luz verde encendida

    [Header("Tiempos de Secuencia")]
    public float startDelay = 1.0f;
    public float stepInterval = 1.0f;

    void Start()
    {
        // Apagamos todas las luces al iniciar
        UpdateAllLights(allOffSprite);
        StartCoroutine(StartRaceSequence());
    }

    // Función auxiliar para cambiar el sprite en todos los semáforos a la vez
    void UpdateAllLights(Sprite newSprite)
    {
        foreach (SpriteRenderer renderer in trafficLightRenderers)
        {
            if (renderer != null && newSprite != null)
            {
                renderer.sprite = newSprite;
            }
        }
    }

    IEnumerator StartRaceSequence()
    {
        if (kartController != null) kartController.canMove = false;

        yield return new WaitForSeconds(startDelay);

        // 🔴 LUZ ROJA
        Debug.Log("🔴 ROJO");
        UpdateAllLights(redLightSprite);
        yield return new WaitForSeconds(stepInterval);

        // 🟡 LUZ AMARILLA
        Debug.Log("🟡 AMARILLO");
        UpdateAllLights(yellowLightSprite);
        yield return new WaitForSeconds(stepInterval);

        // 🟢 LUZ VERDE
        Debug.Log("🟢 VERDE - ¡YA!");
        UpdateAllLights(greenLightSprite);

        // Arranca el kart
        if (kartController != null)
        {
            kartController.canMove = true;
        }
    }
}
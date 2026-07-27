using UnityEngine;
using UnityEngine.UI;

public class CharacterMaterialManager : MonoBehaviour
{
    [Header("Kart Fronts")]
    [SerializeField] private MeshRenderer playerFront;
    [SerializeField] private MeshRenderer cpuFront;

    [Header("HUD References")]
    [SerializeField] private Image playerBar;
    [SerializeField] private Image cpuBar;

    [Header("Kart Materials")]
    [SerializeField] private Material[] kartMaterials;

    private void Start()
    {
        ApplyMaterials();
    }

    private void ApplyMaterials()
    {
        if (kartMaterials == null || kartMaterials.Length < 8)
        {
            Debug.LogError("Please assign all 8 kart materials.");
            return;
        }

        int playerIndex = GameData.PlayerSelection;
        int cpuIndex = GameData.CpuSelection;

        // Change kart colors
        if (playerFront != null)
            playerFront.material = kartMaterials[playerIndex];

        if (cpuFront != null)
            cpuFront.material = kartMaterials[cpuIndex];

        // Change HUD colors
        if (playerBar != null)
            playerBar.color = kartMaterials[playerIndex].color;

        if (cpuBar != null)
            cpuBar.color = kartMaterials[cpuIndex].color;
    }
}

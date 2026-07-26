using System.Collections;
using UnityEngine;
using UnityEngine.Device;


public class TrafficLightManager : MonoBehaviour
{
    [Header("Controllers")]
    public KartLaneController playerKartController;
    public CPUController cpuController;

    [Header("Traffic lights\r\n")]
    public SpriteRenderer[] trafficLightRenderers;

    [Header("Left Traffic Light Sprites")]
    public Sprite leftAllOffSprite;
    public Sprite leftRedSprite;
    public Sprite leftYellowSprite;
    public Sprite leftGreenSprite;

    [Header("Right Traffic Light Sprites")]
    public Sprite rightAllOffSprite;
    public Sprite rightRedSprite;
    public Sprite rightYellowSprite;
    public Sprite rightGreenSprite;

    [Header("Time")]
    public float startDelay = 1f;
    public float stepInterval = 1f;

    private bool isPlayingSequence = false;

    //Starts the countdown for the player's turn.
    public void StartPlayerTurnSequence()
    {


        if (isPlayingSequence) return;
        StartCoroutine(StartRaceSequence(GameManager.GameState.PlayerTurn));
    }
    //Starts the countdown for the CPU's turn.
    public void StartCPUTurnSequence()
    {
        if (isPlayingSequence) return;
        StartCoroutine(StartRaceSequence(GameManager.GameState.CPUTurn));
    }
    //Main coroutine that handles the timing and visual change of the lights.
    private IEnumerator StartRaceSequence(GameManager.GameState targetTurn)
    {
        isPlayingSequence = true;

        // Lock both before the countdown
        if (playerKartController != null) playerKartController.canMove = false;
        if (cpuController != null) cpuController.canMove = false;

        yield return new WaitForSeconds(startDelay);

        UpdateLights(leftRedSprite, rightRedSprite);
        Debug.Log("RED");
        yield return new WaitForSeconds(stepInterval);

        UpdateLights(leftYellowSprite, rightYellowSprite);
        Debug.Log("YELLOW");
        yield return new WaitForSeconds(stepInterval);

        UpdateLights(leftGreenSprite, rightGreenSprite);
        Debug.Log("GREEN");

        // Changes the official turn
        GameManager.Instance.gameState = targetTurn;

        // Activate ONLY the one that applies
        if (targetTurn == GameManager.GameState.PlayerTurn)
        {
            if (playerKartController != null) playerKartController.canMove = true;
            if (cpuController != null) cpuController.canMove = false;
        }
        else if (targetTurn == GameManager.GameState.CPUTurn)
        {
            if (cpuController != null) cpuController.canMove = true;
            if (playerKartController != null) playerKartController.canMove = false;
        }

        isPlayingSequence = false;
    }

    //Helper method that changes the current sprite of all traffic lights on the screen.
    private void UpdateLights(Sprite leftSprite, Sprite rightSprite)
    {
        if (trafficLightRenderers.Length > 0 && trafficLightRenderers[0] != null && leftSprite != null)
        {
            trafficLightRenderers[0].sprite = leftSprite;
        }

        if (trafficLightRenderers.Length > 1 && trafficLightRenderers[1] != null && rightSprite != null)
        {
            trafficLightRenderers[1].sprite = rightSprite;
        }
    }
}
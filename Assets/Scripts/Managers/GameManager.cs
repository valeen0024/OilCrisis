using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.InputManagerEntry;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Traffic Light")]
    public TrafficLightManager trafficLightManager;

    [Header("UI")]
    [SerializeField] private GameObject uiInstructions;
    [SerializeField] private GameObject uiPlay;
    [SerializeField] private GameObject uiWin;

    [SerializeField]
    [Range(1f, 5f)]
    private float instructionsTime = 3f;

    public enum GameState
    {
        Waiting,
        PlayerTurn,
        CPUTurn,
        Finished
    }

    private List<GameObject> fuelBarrels = new List<GameObject>();

    public GameState gameState;
    public bool isGameOver;
    public bool playerWon;
    public string winnerName;

    [HideInInspector]
    public float finalPlayerDistance;

    [HideInInspector]
    public float finalCPUDistance;

    //Initializes the Singleton pattern to ensure only one GameManager exists in the scene.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //Called at the start of the game.Starts the match and finds the fuel barrels.
    private void Start()
    {
        FindFuelBarrels();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent<KartLaneController>(out var laneCtrl))
        {
            laneCtrl.canMove = false;
        }

        GameObject cpu = GameObject.FindGameObjectWithTag("CPU");
        if (cpu != null && cpu.TryGetComponent<CPUController>(out var cpuCtrl))
        {
            cpuCtrl.canMove = false;
        }

        StartCoroutine(ShowInstructionsRoutine());
    }

    private IEnumerator ShowInstructionsRoutine()
    {
        gameState = GameState.Waiting;

        if (uiPlay != null)
            uiPlay.SetActive(false);

        if (uiWin != null)
            uiWin.SetActive(false);

        if (uiInstructions != null)
            uiInstructions.SetActive(true);

        yield return new WaitForSeconds(instructionsTime);

        if (uiInstructions != null)
            uiInstructions.SetActive(false);

        if (uiPlay != null)
            uiPlay.SetActive(true);

        StartGame();
    }


    //Finds all objects with the "Fuel" tag in the scene and stores them in a list.
    private void FindFuelBarrels()
    {
        GameObject[] foundBarrels = GameObject.FindGameObjectsWithTag("Fuel");
        foreach (GameObject barrel in foundBarrels)
        {
            fuelBarrels.Add(barrel);
        }
    }

    //Resets game variables and starts the traffic light sequence for the player's turn.
    public void StartGame()
    {
        gameState = GameState.Waiting;
        isGameOver = false;
        playerWon = false;
        winnerName = "";
        finalPlayerDistance = 0f;
        finalCPUDistance = 0f;

        if (trafficLightManager != null)
        {
            trafficLightManager.StartPlayerTurnSequence();
        }
    }

    //Reactivates all saved fuel barrels in the list for the next turn.
    public void RespawnFuelBarrels()
    {
        foreach (GameObject barrel in fuelBarrels)
        {
            if (barrel != null)
            {
                barrel.SetActive(true);
            }
        }
    }

    //Checks if it is the player's turn and starts the transition coroutine to the CPU.
    public void EndPlayerTurn()
    {
        if (gameState != GameState.PlayerTurn) return;

        // Instead of an abrupt transition, we initiate the cinematic shift-change sequence.
        StartCoroutine(TransitionToCPUTurn());
    }

    private IEnumerator TransitionToCPUTurn()
    {
        // Change the status to waiting
        gameState = GameState.Waiting;

        // Stop the player immediately
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Turn off lane control
            if (player.TryGetComponent<KartLaneController>(out var laneCtrl))
            {
                laneCtrl.canMove = false;
                laneCtrl.StopEngineSound();
            }

            // Disable physics-based movement
            if (player.TryGetComponent<KartMovement>(out var kartMov))
            {
                kartMov.enabled = false;
            }

            // Stop the Rigidbody and freeze physics
            if (player.TryGetComponent<Rigidbody>(out var rb))
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                rb.isKinematic = true;
            }
        }

        Debug.Log("¡Turn over! The car has stopped. Waiting 3 seconds...");

        // 3-second wait before changing turns
        yield return new WaitForSeconds(3f);

        // Hide the player
        if (player != null)
        {
            player.SetActive(false);
            Debug.Log("Hidden player's kart.");
        }

        // Point the camera at the CPU
        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        GameObject cpu = GameObject.FindGameObjectWithTag("CPU");

        if (cam != null && cpu != null)
        {
            cam.SetTarget(cpu.transform);
            Debug.Log("Camera switching to the CPU kart.");
        }

        // Reset the fuel barrels
        RespawnFuelBarrels();

        // Start the CPU traffic light
        if (trafficLightManager != null)
        {
            trafficLightManager.StartCPUTurnSequence();
        }
    }
    // Ends the CPU's turn and calls the function to end the game.
    public void EndCPUTurn()
    {
        if (gameState != GameState.CPUTurn) return;

        GameObject cpu = GameObject.FindGameObjectWithTag("CPU");
        if (cpu != null && cpu.TryGetComponent<CPUController>(out var cpuCtrl))
        {
            cpuCtrl.StopEngineSound();
        }

        Debug.Log("CPU turn finished.");
        FinishGame();
    }

    //The turn ends when the player runs out of fuel.

    public void PlayerOutOfFuel()
    {
        EndPlayerTurn();
    }
    // The turn ends when the CPU runs out of fuel.

    public void CPUOutOfFuel()
    {
        EndCPUTurn();
    }
    // Changes the game state to finished and runs the check to see who won.
    public void FinishGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        gameState = GameState.Finished;
        CheckWinner();

        Debug.Log(
            "Game finished. Winner: " +
            winnerName
        );

    }

    // Compares the final distance of the player and the CPU to declare a winner or a draw.

    public void CheckWinner()
    {

        Debug.Log(
            "Comparing distances - Player: " +
            finalPlayerDistance +
            " | CPU: " +
            finalCPUDistance
        );

        if (finalPlayerDistance > finalCPUDistance)
        {
            winnerName = "Player";
            playerWon = true;
        }
        else if (finalCPUDistance > finalPlayerDistance)
        {
            winnerName = "CPU";
            playerWon = false;
        }
        else
        {
            winnerName = "Draw";
            playerWon = false;
        }
    }

    // Reloads the current scene, restarting the race from scratch.
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //Loads the Main Menu scene.
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
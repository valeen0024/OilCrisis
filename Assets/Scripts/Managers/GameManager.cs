using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameManager : MonoBehaviour
{
    // Static instance to implement the Singleton pattern and access the GameManager from any script
    public static GameManager Instance;

    // Definition of the possible states in which the game can be
    public enum GameState
    {
        Waiting,    // Definition of the possible states in which the game can be
        PlayerTurn, // Active player's turn
        CPUTurn,    // Artificial Intelligence's active turn
        Finished    // Game over
    }

    [Header("General State of the Game")]
    public GameState gameState; 
    public bool isGameOver;     
    public bool playerWon;      
    public string winnerName;

    // Variables hidden in the Inspector where the 'DistanceTracker' scripts store the final distance traveled
    [HideInInspector]
    public float finalPlayerDistance;

    [HideInInspector]
    public float finalCPUDistance;

    [Header("Vehicles (References)")]
    // Complete player kart object
    public GameObject playerKart;

    // Complete CPU kart object
    public GameObject cpuKart;


    // Executes before Start. Implements the Singleton pattern to ensure a single instance.
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // If a GameManager does not exist, this one is assigned as the main one.
        }
        else
        {
            Destroy(gameObject); // If another one already exists in the scene, destroy this duplicate
        }
    }

    // Executes on the first frame. Starts the initial game logic.
    //private void Start()
    //{
    //    StartGame();

    //    Debug.Log("=== GAME STARTED ===");
    //    Debug.Log("Game State: " + gameState);
    //}

    // Resets the variables to their initial values ​​and assigns the first turn to the player
    public void StartGame()
    {
        gameState = GameState.PlayerTurn;
        isGameOver = false;
        playerWon = false;
        winnerName = "";
        finalPlayerDistance = 0f;
        finalCPUDistance = 0f;

        // Ensure both vehicles are visible when starting a new game
        if (playerKart != null)
        {
            playerKart.SetActive(true);
        }

        if (cpuKart != null)
        {
            cpuKart.SetActive(true);
        }

        Debug.Log("Game started. Player turn.");
    }

    // Ends the player's turn and begins the switch to the CPU's turn.
    public void EndPlayerTurn()
    {
        // Security validation: proceed only if it is currently the player's turn
        if (gameState != GameState.PlayerTurn)
            return;

        // Starts the coroutine that handles the delay before the CPU plays
        StartCoroutine(StartCPUTurnAfterDelay());
    }

    // Hides the player's kart object in the scene
    public void HidePlayerKart()
    {
        if (playerKart != null)
        {
            playerKart.SetActive(false);
            Debug.Log("Player kart hidden.");
        }
    }

    // Displays the player's kart object in the scene
    public void ShowPlayerKart()
    {
        if (playerKart != null)
        {
            playerKart.SetActive(true);
            Debug.Log("Player kart shown.");
        }
    }

    // Coroutine that generates a brief pause between the player's turn and the CPU's turn
    private IEnumerator StartCPUTurnAfterDelay()
    {
        // Switches to standby mode to stop the movement of both vehicles
        gameState = GameState.Waiting;

        Debug.Log("Player turn finished.");
        Debug.Log("CPU turn starts in 3 seconds.");

        // Pauses the execution of this method for 3 real-time game seconds
        yield return new WaitForSeconds(3f);

        // Hides the player's kart at the end of their turn to make way for the CPU's kart.
        HidePlayerKart();

        // Officially changes the state to the CPU's turn
        gameState = GameState.CPUTurn;

        Debug.Log("CPU turn begins.");
    }

    // Hides the CPU kart object in the scene
    public void HideCPUKart()
    {
        if (cpuKart != null)
        {
            cpuKart.SetActive(false);
            Debug.Log("CPU kart hidden.");
        }
    }

    // Displays the CPU kart object in the scene
    public void ShowCPUKart()
    {
        if (cpuKart != null)
        {
            cpuKart.SetActive(true);
            Debug.Log("CPU kart shown.");
        }
    }

    // Ends the CPU's turn and proceeds to finish the race
    public void EndCPUTurn()
    {
        // Security validation: proceed only if it is currently the CPU's turn
        if (gameState != GameState.CPUTurn)
            return;

        Debug.Log("CPU turn finished.");

        // Finaliza el juego por completo
        FinishGame();
    }

    // Called externally when the player runs out of fuel
    public void PlayerOutOfFuel()
    {
        Debug.Log(
            "Player ran out of fuel at " +
            finalPlayerDistance +
            " yards."
        );

        EndPlayerTurn();
    }

    // It is called externally when the CPU runs out of fuel.
    public void CPUOutOfFuel()
    {
        Debug.Log(
            "CPU ran out of fuel at " +
            finalCPUDistance +
            " yards."
        );

        EndCPUTurn();
    }

    // Changes the game state to 'Finished' and triggers the determination of the winner
    public void FinishGame()
    {
        // If the game had already ended, the logic is not executed again.
        if (isGameOver)
            return;

        isGameOver = true;
        gameState = GameState.Finished;

        // Compares the saved results to declare a winner
        CheckWinner();

        Debug.Log(
            "Game finished. Winner: " +
            winnerName
        );
    }

    // Compares the final recorded distances of both participants to determine who won
    public void CheckWinner()
    {
        Debug.Log(
            "Comparing distances - Player: " +
            finalPlayerDistance +
            " | CPU: " +
            finalCPUDistance
        );

        // Player Victory
        if (finalPlayerDistance > finalCPUDistance)
        {
            winnerName = "Player";
            playerWon = true;
        }
        // CPU Victory
        else if (finalCPUDistance > finalPlayerDistance)
        {
            winnerName = "CPU";
            playerWon = false;
        }
        // Draw
        else
        {
            winnerName = "Draw";
            playerWon = false;
        }
    }

    // Reloads the current scene to restart the race from scratch
    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // Loads the main menu scene to exit game mode
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(
            "MainMenu"
        );
    }
}
using UnityEngine;

public class DistanceTracker : MonoBehaviour
{
    private Transform startLine;
    private Transform finishLine;
    public bool isPlayer;
    public float currentDistance;
    public float maxDistance = 200f;
    public float distanceMultiplier = 3.4f;

    private bool hasFinished = false;


    private void Start()
    {
        hasFinished = false;

        // Automatically finds the start line using its Tag.
        GameObject startObject =
            GameObject.FindGameObjectWithTag("Start");


        // Automatically locates the finish line using its tag.
        GameObject finishObject =
            GameObject.FindGameObjectWithTag("Finish");


        // Stores the reference to the starting line.
        if (startObject != null)
        {
            startLine = startObject.transform;

            Debug.Log(
                gameObject.name +
                " encontró la línea de inicio: " +
                startLine.name
            );
        }
        else
        {
            Debug.LogError(
                gameObject.name +
                ": no se encontró un objeto con el Tag 'Start'."
            );
        }


        // Stores the reference to the finish line.
        if (finishObject != null)
        {
            finishLine = finishObject.transform;

            Debug.Log(
                gameObject.name +
                " encontró la línea de meta: " +
                finishLine.name
            );
        }
        else
        {
            Debug.LogError(
                gameObject.name +
                ": no se encontró un objeto con el Tag 'Finish'."
            );
        }
    }

    private void Update()
    {
        // Prevents execution if the GameManager instance does not exist.
        if (GameManager.Instance == null) return;

        // Checks if it is currently this vehicle's active turn.
        bool isMyTurn = (isPlayer && GameManager.Instance.gameState == GameManager.GameState.PlayerTurn) ||
                        (!isPlayer && GameManager.Instance.gameState == GameManager.GameState.CPUTurn);
        
        // Executes distance calculations and finish checks only during its turn and before reaching the finish line.
        if (isMyTurn && !hasFinished)
        {
            UpdateDistance();
            CheckFinish();
        }
    }

    public void UpdateDistance()
    {
        // Calculates the distance between the kart and the start line

        float rawDistance =
            Mathf.Abs(
                transform.position.z -
                startLine.position.z
            );


        // Converts the Unity distance to yards.
        currentDistance =
            rawDistance *
            distanceMultiplier;


        // Rounds the distance to two decimal places.
        currentDistance =
            Mathf.Round(
                currentDistance * 100f
            ) / 100f;


        Debug.Log(
            gameObject.name +
            " | Distance: " +
            currentDistance.ToString("F2") +
            " / Max: " +
            maxDistance
        );


        // Stores the corresponding distance in the GameManager.
        if (isPlayer)
        {
            GameManager.Instance.finalPlayerDistance =
                currentDistance;
        }
        else
        {
            GameManager.Instance.finalCPUDistance =
                currentDistance;
        }
    }

    public void CheckFinish()
    {
        // Evaluates if the current distance has met or exceeded the required maximum distance.
        if (currentDistance >= maxDistance)
        {
            // Clamps the current distance to the maximum limit.
            currentDistance = maxDistance;

            // Flags the vehicle as finished to stop future updates.
            hasFinished = true;
            
            // Notifies the GameManager to wrap up the respective turn.
            if (isPlayer)
            {
                Debug.Log("Player reached the 200 yards finish line!");
                GameManager.Instance.EndPlayerTurn();
            }
            else
            {
                Debug.Log("CPU reached the 200 yards finish line!");
                GameManager.Instance.EndCPUTurn();
            }
        }
    }
}
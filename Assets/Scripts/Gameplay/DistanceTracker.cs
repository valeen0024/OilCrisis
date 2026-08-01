using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class DistanceTracker : MonoBehaviour
{
    private Transform startLine;
    public bool isPlayer;
    public float currentDistance;
    public float maxDistance = 200f;
    public float distanceMultiplier = 3.4f;
    private bool hasFinished = false;

    //Executed once when the game starts.
    private void Start()
    {
        // Ensure that the initial state is not finished.
        hasFinished = false;

        // Finds the object with the "Start" tag in the scene.
        GameObject startObject = GameObject.FindGameObjectWithTag("Start");

        // If found, saves its Transform
        if (startObject != null)
        {
            startLine = startObject.transform;
        }
        else
        {
            Debug.LogError("No object with the tag 'Start' was found.");
        }
    }

    //Executed every frame of the game.
    private void Update()
    {

        //Checks if it's this specific kart's turn by evaluating the GameManager's state.
        bool isMyTurn =
            (isPlayer && GameManager.Instance.gameState == GameManager.GameState.PlayerTurn)
            ||
            (!isPlayer && GameManager.Instance.gameState == GameManager.GameState.CPUTurn);


        //f it's this kart's turn and it hasn't reached the finish line yet, calculate its progress.
        if (isMyTurn && !hasFinished)
        {
            UpdateDistance();
            CheckFinish();
        }
    }

    //Calculates the distance based on the kart's Z position relative to the start line's Z position.
    public void UpdateDistance()
    {

        //Gets the absolute difference on the Z axis and multiplies it by the scale factor.
        currentDistance = Mathf.Abs(transform.position.z - startLine.position.z) * distanceMultiplier;
        //Ensures the current distance never exceeds the maximum limit
        currentDistance = Mathf.Min(currentDistance, maxDistance);

        Debug.Log(gameObject.name + " has covered: " + currentDistance.ToString("F1") + " yards.");

        //Updates the final distance in the GameManager depending on who this kart is.
        if (isPlayer)
        {
            GameManager.Instance.finalPlayerDistance = currentDistance;
        }
        else
        {
            GameManager.Instance.finalCPUDistance = currentDistance;
        }
    }


    //Checks if the kart has reached the required distance to finish its race.
    public void CheckFinish()
    {
        //Marks the kart as finished and sets its distance to the exact maximum.
        if (currentDistance >= maxDistance)
        {
            hasFinished = true;
            currentDistance = maxDistance;

            Debug.Log(gameObject.name + " It reached 200 yards and stopped.");

            // Stopping logic if it is the player.
            if (isPlayer)
            {
                // Turns off lane control so it doesn't receive keyboard commands.
                if (TryGetComponent<KartLaneController>(out var laneController))
                {
                    laneController.canMove = false;
                }

                // Turns off the physics movement script if equipped.
                if (TryGetComponent<KartMovement>(out var kartMovement))
                {
                    kartMovement.enabled = false;

                    laneController.StopEngineSound();
                }

                // Immediately eliminate any physical velocity or inertia.
                if (TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true; // It freezes it completely in space.
                }
                //Notifies the GameManager that the player's turn ended successfully.
                GameManager.Instance.EndPlayerTurn();
            }

            //Stopping logic if it is the CPU.
            else
            {
                //Turns off its movement artificial intelligence.
                if (TryGetComponent<CPUController>(out var cpuController))
                {
                    cpuController.canMove = false;

                    cpuController.StopEngineSound();
                }

                //Stops its physics completely.
                if (TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
                //Notifies the GameManager that the CPU's turn ended successfully.
                GameManager.Instance.EndCPUTurn();
            }
        }
    }
}
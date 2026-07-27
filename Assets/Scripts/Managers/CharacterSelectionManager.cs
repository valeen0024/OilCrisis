using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Selection Borders")]
    [SerializeField] private RectTransform playerBorder;
    [SerializeField] private RectTransform cpuBorder;

    [Header("Color Points")]
    [SerializeField] private Transform[] colorPoints;

    [Header("Locked Overlays")]
    [SerializeField] private GameObject[] lockOverlays;

    [Header("CPU Settings")]
    [SerializeField] private float cpuMoveSpeed = 0.08f;
    [SerializeField] private float cpuThinkingTime = 1.2f;


    private int currentSelection = 0;

    private bool playerConfirmed = false;
    private bool inputEnabled = true;

    private int playerSelection = -1;
    private int cpuSelection = -1;
    private int cpuCurrentIndex = 0;


    private void Start()
    {
        cpuBorder.gameObject.SetActive(false);

        for (int i = 0; i < lockOverlays.Length; i++)
        {
            lockOverlays[i].SetActive(false);
        }

        UpdatePlayerBorder();
    }

    private void Update()
    {
        if (!inputEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveRight();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveUp();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveDown();
        }
    }

    private void MoveRight()
    {
        if (currentSelection % 4 < 3)
        {
            currentSelection++;
            UpdatePlayerBorder();
        }
    }

    private void MoveLeft()
    {
        if (currentSelection % 4 > 0)
        {
            currentSelection--;
            UpdatePlayerBorder();
        }
    }

    private void MoveUp()
    {
        Debug.Log("MoveUp ejecutado");

        if (currentSelection >= 4)
        {
            currentSelection -= 4;
            UpdatePlayerBorder();
        }
    }

    private void MoveDown()
    {
        Debug.Log("MoveDown ejecutado");

        if (currentSelection < 4)
        {
            currentSelection += 4;
            UpdatePlayerBorder();
        }
    }

    private void UpdatePlayerBorder()
    {
        Debug.Log("Current Selection: " + currentSelection);

        playerBorder.position = colorPoints[currentSelection].position;
    }

    public void ConfirmSelection()
    {
        if (playerConfirmed)
            return;

        playerConfirmed = true;
        inputEnabled = false;

        playerSelection = currentSelection;

        lockOverlays[playerSelection].SetActive(true);

        cpuBorder.gameObject.SetActive(true);

        StartCoroutine(CPUSelectionRoutine());
    }

    public void RandomSelection()
    {
        if (playerConfirmed)
            return;

        currentSelection = Random.Range(0, colorPoints.Length);

        UpdatePlayerBorder();

        ConfirmSelection();
    }
    private IEnumerator CPUSelectionRoutine()
    {
        do
        {
            cpuSelection = Random.Range(0, colorPoints.Length);

        } while (cpuSelection == playerSelection);

        cpuCurrentIndex = 0;

        float timer = 0f;

        while (timer < cpuThinkingTime)
        {
            cpuBorder.position = colorPoints[cpuCurrentIndex].position;

            cpuCurrentIndex++;

            if (cpuCurrentIndex >= colorPoints.Length)
                cpuCurrentIndex = 0;

            timer += cpuMoveSpeed;

            yield return new WaitForSeconds(cpuMoveSpeed);
        }

        cpuBorder.position = colorPoints[cpuSelection].position;

        lockOverlays[cpuSelection].SetActive(true);

        GameData.PlayerSelection = playerSelection;
        GameData.CpuSelection = cpuSelection;

        Debug.Log("Llegué al final de la Coroutine");

        yield return new WaitForSeconds(0.8f);

        Debug.Log("Voy a cargar Gameplay");

        SceneManager.LoadScene("Gameplay");
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject creditsPanel;

    [Header("Scenes")]
    [SerializeField] private string characterSelectionScene = "CharacterSelection";

    private void Start()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(characterSelectionScene);
    }

    public void OpenCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
    }

    public void CloseCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    public void ExitGame()
    {
        Debug.Log("Closing game...");

        Application.Quit();
    }
}
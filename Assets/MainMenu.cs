using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
    }

    public void StartNewGame()
    {
        // Delete old save and start fresh
        SaveSystem.DeleteSave();
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        if (SaveSystem.SaveExists())
        {
            // Load save data into GameManager and then go to game scene
            SaveData data = SaveSystem.LoadGame();
            if (data != null)
            {
                GameManager.loadedSaveData = data;
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                Debug.LogWarning("Save file exists but could not be loaded.");
            }
        }
        else
        {
            Debug.Log("No save found");
        }
    }

    public void SaveAndQuit()
    {
        // Save the game before quitting
        SaveSystem.SaveGame();

        // Load main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        // Save the game automatically before quitting
        SaveSystem.SaveGame();

        // Quit the application
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
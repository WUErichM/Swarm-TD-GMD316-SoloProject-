using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject panel;

    void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0f)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ResetRun()
    {
        Time.timeScale = 1f;
        SaveSystem.DeleteSave();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SaveAndQuit()
    {
        Time.timeScale = 1f;
        SaveSystem.SaveGame();
        SceneManager.LoadScene("MainMenu");
    }
}
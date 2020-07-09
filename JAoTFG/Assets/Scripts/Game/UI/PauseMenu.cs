using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{

    public GameObject pauseMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        bool toggle = !pauseMenu.activeInHierarchy;
        pauseMenu.SetActive(toggle);

        if (toggle)
        {
            GameManager.instance.PauseGame();
        }
        else
        {
            GameManager.instance.ResumeGame();
        }
    }

    public void BTN_ExitMenu()
    {
        pauseMenu.SetActive(false);

        GameManager.instance.ResumeGame();
    }

    public void BTN_Resume()
    {
        pauseMenu.SetActive(false);

        GameManager.instance.ResumeGame();
    }

    public void BTN_Restart()
    {
        GameManager.instance.ReloadLevel();
    }

    public void BTN_MainMenu()
    {
        GameManager.instance?.ChangeGameMode(Gamemode.mainMenu);
    }

    public void BTN_ExitGame()
    {
        if (Application.isEditor) return;

        Application.Quit();
    }

}

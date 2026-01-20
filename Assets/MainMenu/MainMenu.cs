using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject menuScreen;
    public GameObject levelSelectScreen;
    public GameObject settingsScreen;

    public GameObject levelButtons;

    private void Start()
    {
        levelSelectScreen.SetActive(false);
        settingsScreen.SetActive(false);
        menuScreen.SetActive(true);

        DisableButtons();
    }

    void DisableButtons()
    {
        int levelProgress = FindFirstObjectByType<GameManager>().levelProgress;

        if(levelButtons == null) return;

        Button[] buttons = levelButtons.GetComponentsInChildren<Button>();

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < levelProgress)
            {
                buttons[i].interactable = true;
            }
            else
            {
                buttons[i].interactable = false;
            }
        }
    }

    //public void PlayGame()
    //{
    //    //UnityEngine.SceneManagement.SceneManager.LoadScene(
    //        //UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);

    //    menuScreen.SetActive(false);
    //    levelSelectScreen.SetActive(true);
    //}

    //public void GameSettings() 
    //{
    //    menuScreen.SetActive(false);
    //    settingsScreen.SetActive(true);
    //}

    //public void BackToMain()
    //{
    //    menuScreen.SetActive(true);
    //    settingsScreen.SetActive(false);
    //    levelSelectScreen.SetActive(false);
    //}

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}

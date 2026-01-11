using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject menuScreen;
    public GameObject levelSelectScreen;
    public GameObject settingsScreen;

    private void Start()
    {
        levelSelectScreen.SetActive(false);
        settingsScreen.SetActive(false);
        menuScreen.SetActive(true);
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

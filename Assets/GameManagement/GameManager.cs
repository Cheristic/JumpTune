using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public LevelData[] levels;
    int selectedLevel = -1;
    public int levelProgress = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        DontDestroyOnLoad(this);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public void SwapToLevel(int selected)
    {
        selectedLevel = selected-1;

        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "LevelScene") return;

        if (selectedLevel < 0 || selectedLevel > levels.Length - 1) return;

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();

        if (levelManager != null)
        {
            levelManager.LoadFromManager(levels[selectedLevel]);
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SwapToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}

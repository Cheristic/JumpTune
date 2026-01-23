using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    internal SaveManager SaveManager;
    public LevelData[] levels;
    internal int selectedLevel = 0;
    public int levelProgress = 1;

    public static event Action EndGame;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        else Instance = this;
        DontDestroyOnLoad(this);

        SaveManager = new(levels);

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

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadFromManager(levels[selectedLevel]);
        }
    }

    public void SwapToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    [ContextMenu("Delete Save Data")]
    public void DeleteSaveDataInternal()
    {
        SaveManager = new(levels);
        SaveManager.DeleteSaveData();
    }
    public void ResetSaveData()
    {
        SaveManager.ResetData();
    }

    public void TriggerEndGame()
    {
        EndGame?.Invoke();
    }
}

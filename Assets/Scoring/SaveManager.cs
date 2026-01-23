using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;


public class SaveManager
{
    [Serializable]
    public class LevelScore
    {
        public int rank;
        public int score;
        public float time;
    }
    [Serializable]
    public class SaveData
    {
        public List<LevelScore> levels;
        [JsonConstructor]
        public SaveData(int count)
        {
            if (count > 0 )
            {
                levels = new List<LevelScore>();
                for (int i = 0; i < count; i++) { levels.Add(new LevelScore()); }
            }

        }
        public void Reset()
        {
            int count = levels.Count;
            levels = new List<LevelScore>();
            for (int i = 0; i < count; i++) { levels.Add(new LevelScore()); }
        }

    }

    string dataFileName = "NONONO";

    internal SaveData CurrData;
    public SaveManager(LevelData[] levels)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, dataFileName);
        if (File.Exists(fullPath))
        {
            try
            {
                string gameDataLoaded = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using StreamReader reader = new StreamReader(stream);
                    gameDataLoaded = reader.ReadToEnd();
                }
                CurrData = JsonConvert.DeserializeObject<SaveData>(gameDataLoaded);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured while loading game data: " + e);
                CurrData = new(levels.Length);
            }
        }
        else
        {
            CurrData = new(levels.Length);
        }
    }
    [DllImport("__Internal")]
    private static extern void JS_FileSystem_Sync();
    public void Save()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, dataFileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string gameDataJSON = JsonConvert.SerializeObject(CurrData);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
#if UNITY_WEBGL
                    if (!Application.isEditor) JS_FileSystem_Sync();
#endif
                    writer.Write(gameDataJSON);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Game data save error: " + e);
        }
    }

    public void DeleteSaveData()
    {
        try
        {
            File.Delete(Path.Combine(Application.persistentDataPath, dataFileName));
        }
        catch (Exception e)
        {
            Debug.LogError("Game data delete error: " + e);
        }
    }
    public void ResetData()
    {
        CurrData.Reset();
        Save();
    }

    public void CompleteLevel(int rank, int score, float time)
    {
        if (score > CurrData.levels[GameManager.Instance.selectedLevel].score)
        {
            CurrData.levels[GameManager.Instance.selectedLevel].rank = rank;
            CurrData.levels[GameManager.Instance.selectedLevel].score = score;
            CurrData.levels[GameManager.Instance.selectedLevel].time = time;
        }

        Save();
    }
}

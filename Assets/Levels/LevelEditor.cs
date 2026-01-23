using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
[CustomEditor(typeof(LevelManager))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelManager levelManager = (LevelManager)target;
        
        if(GUILayout.Button("Create level from preset"))
        {
            levelManager.LoadFromEditor();
        }

        if (GUILayout.Button("Clear level"))
        {
            if (Application.isPlaying) return;
            levelManager.ClearLoadedLevel();
        }
    }
}
#endif

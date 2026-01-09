using UnityEngine;
using UnityEditor;

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
    }
}

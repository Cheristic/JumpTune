using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public List<TileData> tiles;
    public int notchCount;
    public float levelWidth;
    public float towerWidth;
    public float centSpacing;

    public string Title;
    public int tuningSystem;
}

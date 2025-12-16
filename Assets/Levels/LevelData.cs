using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public List<TileData> tiles;
    public Vector2 playerStartPosition;
    public int notchCount;
    public float levelWidth;
}

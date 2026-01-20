using UnityEngine;

[System.Serializable]
public class TileData
{
    public bool isFixed;
    public int correctFrequencyIdx;
    public bool hasBreak;
    public bool endsChunk; // if hasBreak == true, this should always be true
}

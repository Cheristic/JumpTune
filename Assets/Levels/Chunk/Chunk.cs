using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Transform Shaker;

    [SerializeField] float shakeFrequency;
    [SerializeField] float shakeMult;
    [SerializeField] float shakeExp;
    [SerializeField] float finalSpeedMult;

    List<TonePlatform> platforms;

    LevelManager _man;
    internal int chunkIndex;

    internal Vector2 ChunkBounds;
    
    public void Init(LevelManager man, int index)
    {
        _man = man;
        platforms = new();
        chunkIndex = index;
        currShakeSpeed = 0;
    }

    public void AppendPlatform(TonePlatform platform)
    {
        platforms.Add(platform);
    }

    public void FinishChunk(float y_end, bool isHeldUp)
    {
        Vector2 size = new Vector2(_man.levelData.levelWidth, y_end - transform.position.y);
        Shaker.GetComponent<SpriteRenderer>().size = new Vector2((size.x - size.x % 4)*2, isHeldUp ? size.y : size.y - size.y % 4);

        float startSpacing = chunkIndex == 0 ? -200f : 0;
        ChunkBounds = new Vector2(transform.position.y + startSpacing, y_end);

        foreach (var platform in platforms) platform.transform.parent = Shaker;
    }

    public int ChunkScore()
    {
        int score = 0;
        foreach (var platform in platforms)
        {
                if (platform.isFixed) continue;
                int error = (int)platform.Error();
                score += System.Math.Max(10 - error, 0);        
        }
        return score;
    }

    public void SetChunkShake(int totalError)
    {
        //Debug.Log(chunkIndex + ": " + totalError);
        currShakeSpeed = totalError;
    }
    public float currShakeSpeed = 0; // public purely for testing in the inspector
    private void Update()
    {
        if (_man == null) return;
        Shaker.position = new Vector2(Mathf.Sin(2f * Mathf.PI * shakeFrequency * Time.time) *
            Mathf.Clamp(shakeMult * Mathf.Pow(currShakeSpeed, shakeExp),
            -_man.levelData.levelWidth + _man.levelData.towerWidth, _man.levelData.levelWidth - _man.levelData.towerWidth) * finalSpeedMult
            , Shaker.position.y);
    }
}
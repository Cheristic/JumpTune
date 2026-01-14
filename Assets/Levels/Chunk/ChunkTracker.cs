using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class ChunkTracker : MonoBehaviour
{
    public static ChunkTracker Instance { get; private set; }
    List<Chunk> Chunks;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
    public void CreateChunks(List<Chunk> chunks)
    {
        Chunks = chunks;
        if (!Application.isPlaying) return;
        StartCoroutine(TrackChunksLoop()); // it's possible this has to be put in start or something
    }
    public int TotalScore()
    {
        int score = 0;
        foreach (var chunk in Chunks) score += chunk.ChunkScore();
        return score;
    }

    public void UpdateChunkShakes(int fromChunk)
    {
        int prevScore = 0;
        for (int i = 1; i < Chunks.Count; i++)
        {
            if (i <= fromChunk) prevScore += Chunks[i - 1].ChunkScore();
            Chunks[i].SetChunkShake(prevScore);
        }
    }

    int currChunkIndex = 0;
    IEnumerator TrackChunksLoop()
    {
        while (true)
        {
            Vector2 currBounds = Chunks[currChunkIndex].ChunkBounds;
            // wait until outside curr chunk bounds
            yield return new WaitUntil(() => (PlayerManager.Instance.ChunkCheckerPoint.position.y < currBounds.x ||
                PlayerManager.Instance.ChunkCheckerPoint.position.y >= currBounds.y) &&
                PlayerManager.Instance.controls.isGrounded);

            if (PlayerManager.Instance.ChunkCheckerPoint.position.y < currBounds.x) currChunkIndex--;
            else currChunkIndex++;

            if (currChunkIndex >= Chunks.Count) break; // reached end

            UpdateChunkShakes(currChunkIndex);

        }


        // TRIGGER ENDING
    }

    public float GetChunkXChange()
    {
        if (currChunkIndex >= Chunks.Count) return 0;

        return Chunks[currChunkIndex].framePosXChange;
    }
}
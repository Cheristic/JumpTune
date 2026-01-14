using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class ChunkTracker : MonoBehaviour
{
    List<Chunk> Chunks;
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

    IEnumerator TrackChunksLoop()
    {
        int currChunkIndex = 0;
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
}
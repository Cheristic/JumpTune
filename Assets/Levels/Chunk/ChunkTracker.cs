using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
public class ChunkTracker : MonoBehaviour
{
    public static ChunkTracker Instance { get; private set; }
    List<Chunk> Chunks;
    internal float LevelTimer = 0;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
    private void Start()
    {
       PlayerManager.Instance.Input.Player.StrumChunk.started += StrumCurrentChunk;
    }
    private void OnDisable()
    {
        PlayerManager.Instance.Input.Player.StrumChunk.started -= StrumCurrentChunk;
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
        LevelTimer = Time.time;
        while (true)
        {
            //Debug.Log(currChunkIndex);
            Vector2 currBounds = Chunks[currChunkIndex].ChunkBounds;

            //Debug.Log(Chunks[currChunkIndex].ChunkBounds.x + " " + PlayerManager.Instance.ChunkCheckerPoint.position.y + " " + Chunks[currChunkIndex].ChunkBounds.y);
            // wait until outside curr chunk bounds
            yield return new WaitUntil(() => (PlayerManager.Instance.ChunkCheckerPoint.position.y < currBounds.x ||
                PlayerManager.Instance.ChunkCheckerPoint.position.y >= currBounds.y) &&
                PlayerManager.Instance.controls.isGrounded);

            if (PlayerManager.Instance.ChunkCheckerPoint.position.y < currBounds.x) currChunkIndex--;
            else currChunkIndex++;

            if (currChunkIndex >= Chunks.Count) break; // reached end

            UpdateChunkShakes(currChunkIndex);

        }
        LevelTimer = Time.time - LevelTimer;
        GameManager.Instance.TriggerEndGame();
    }

    public void StrumCurrentChunk(InputAction.CallbackContext ctx)
    {
        if (currChunkIndex < Chunks.Count) Chunks[currChunkIndex].PlayChunkTones();
    }

    public float GetChunkXChange()
    {
        if (currChunkIndex >= Chunks.Count) return 0;

        return Chunks[currChunkIndex].framePosXChange;
    }

    public Chunk GetChunkByYPos(float pos)
    {
        foreach (var chunk in Chunks)
        {
            if (pos >= chunk.ChunkBounds.x && pos < chunk.ChunkBounds.y) return chunk;
        }
        return null;
    }
}
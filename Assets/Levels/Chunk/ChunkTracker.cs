using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Linq;
public class ChunkTracker : MonoBehaviour
{
    public static ChunkTracker Instance { get; private set; }
    List<Chunk> Chunks;
    internal float LevelTimer = 0;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        GameManager.StartGame += OnStartGame;
    }
    private void OnStartGame()
    {
       PlayerManager.Instance.Input.Player.StrumChunk.started += StrumCurrentChunk;
    }
    private void OnDisable()
    {
        GameManager.StartGame -= OnStartGame;
        PlayerManager.Instance.Input.Player.StrumChunk.started -= StrumCurrentChunk;
    }
    public void CreateChunks(List<Chunk> chunks)
    {
        Chunks = chunks;
        currChunkIndex = 0;
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
        int prevError = 0;
        for (int i = 1; i < Chunks.Count; i++)
        {
            if (i <= fromChunk) prevError += Chunks[i - 1].ChunkError();
            Chunks[i].SetChunkShake(prevError);
        }
    }

    int currChunkIndex = 0;
    IEnumerator TrackChunksLoop()
    {
        LevelTimer = Time.time;
        while (true)
        {
            //Debug.Log(currChunkIndex);
            if (currChunkIndex < 0)
            {
                yield return null; // purely for debugging
                continue;
            }

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

        //Debug.Log("Here " + PlayerManager.Instance.ChunkCheckerPoint.position.y);
        //yield return new WaitUntil(() => PlayerManager.Instance.ChunkCheckerPoint.position.y > LevelManager.Instance.topY);

        LevelTimer = Time.time - LevelTimer;
        GameManager.Instance.TriggerEndGame();
    }

    public void StrumCurrentChunk(InputAction.CallbackContext ctx)
    {
        if (currChunkIndex < Chunks.Count) Chunks[currChunkIndex].PlayChunkTones();
    }

    public float GetChunkXChange(bool final = false)
    {
        if (final) return Chunks[^1].framePosXChange;

        if (currChunkIndex >= Chunks.Count || currChunkIndex < 0) return 0;

        return Chunks[currChunkIndex].framePosXChange;
    }


    public List<TonePlatform> GetAllPlatforms()
    {
        List<TonePlatform> p = new();
        foreach (var chunk in Chunks)
            foreach (var plat in chunk.platforms)
                if (plat.TryGetComponent<TonePlatform>(out var tp)) p.Add(tp);
        return p;
    }
}
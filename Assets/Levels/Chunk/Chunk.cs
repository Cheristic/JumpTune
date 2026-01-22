using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;

public class Chunk : MonoBehaviour
{
    public Transform Shaker;

    [SerializeField] float shakeFrequency;
    [SerializeField] float shakeMult;
    [SerializeField] float shakeExp;
    [SerializeField] float finalSpeedMult;
    [SerializeField] float shakeChangeRate;
    [SerializeField] float noiseStrength;

    private RandomWalk walk1;
    private RandomWalk walk2;
    private RandomWalk walk3;

    [Header("Chunk Playing")]
    [SerializeField] float timeBetweenNotes;

    List<GameObject> platforms;

    LevelManager _man;
    internal int chunkIndex;

    internal Vector2 ChunkBounds;
    
    public void Init(LevelManager man, int index)
    {
        _man = man;
        platforms = new();
        chunkIndex = index;
        currShakeSpeed = 0;

        walk1 = new RandomWalk(noiseStrength);
        walk2 = new RandomWalk(noiseStrength);
        walk3 = new RandomWalk(noiseStrength);
    }

    public void AppendPlatform(GameObject platform)
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

    public int ChunkError() =>
        platforms.Sum(p => {
            p.TryGetComponent<TonePlatform>(out var tp);
            return tp == null ? 0 : tp.Error();});

    public int ChunkScore()
    {
        int score = 0;
        foreach (var platform in platforms)
        {
            if (platform.TryGetComponent<TonePlatform>(out var tp))
            { 
                if (tp.isFixed) continue;
                score += tp.Score();
            }
        }
        return score;
    }

    public void PlayChunkTones(bool showError = false)
    {
        StopAllCoroutines();
        StartCoroutine(Player());

        IEnumerator Player() 
        {
            foreach (var platform in platforms)
            {
                if (platform.TryGetComponent<TonePlatform>(out var tp))
                {
                    if (showError && !tp.isFixed) tp.ShowError();
                    tp.PlayPlatformTone();
                    if (tp.hasPlayer) yield break;
                    yield return new WaitForSeconds(timeBetweenNotes);
                }
            }
        }
    }

    float targetShakeSpeed;
    public void SetChunkShake(int totalError)
    {
        //Debug.Log(chunkIndex + ": " + totalError);
        targetShakeSpeed = totalError;
    }
    public float currShakeSpeed = 0; // public purely for testing in the inspector
    internal float framePosXChange = 0;
    private void FixedUpdate()
    {
        if (_man == null) return;

        if (targetShakeSpeed != currShakeSpeed)
        {
            currShakeSpeed = targetShakeSpeed < currShakeSpeed ? Mathf.Clamp(currShakeSpeed - shakeChangeRate, targetShakeSpeed, currShakeSpeed)
                : Mathf.Clamp(currShakeSpeed + shakeChangeRate, currShakeSpeed, targetShakeSpeed);
        }

        float maxX = (_man.levelData.levelWidth - _man.levelData.towerWidth)/2;
        float minX = (-_man.levelData.levelWidth + _man.levelData.towerWidth)/2;

        float ShakeWave(float frequency, float strength, float offset)
            => Mathf.Sin(2f * Mathf.PI * frequency * Time.time + offset)
                * Mathf.Clamp(shakeMult * Mathf.Pow(currShakeSpeed, shakeExp), minX, maxX)
                * finalSpeedMult
                * strength;

        float prevX = Shaker.position.x;
        float newX = ShakeWave(shakeFrequency, 1.0f, walk1.val)
                    + ShakeWave(shakeFrequency * 2.71828f, 0.33f, walk2.val)
                    + ShakeWave(shakeFrequency * 6.28318f, 0.2f, walk3.val);
        // add noise
        // newX += Random.Range(-1, 1) * noiseMult * targetShakeSpeed;
            
        Shaker.position = new Vector2(newX, Shaker.position.y);
        framePosXChange = Shaker.position.x - prevX;
    }

    private class RandomWalk
    {
        public readonly float noiseStrength;
        public float val;

        public RandomWalk(float noiseStrength)
        {
            this.noiseStrength = noiseStrength;
        }

        public void Update() => val += Random.Range(-noiseStrength, noiseStrength);
    }
}
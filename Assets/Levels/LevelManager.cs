using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public LevelData levelData;

    [SerializeField] GameObject tilePrefab;
    [SerializeField] Transform tilesParent;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject startTilePrefab;
    [SerializeField] GameObject endTilePrefab;
    [SerializeField] GameObject bgWallPrefab;
    [SerializeField] GameObject bgWallFixedPrefab;
    [SerializeField] GameObject bgWallEndPrefab;

    [SerializeField] GameObject breakTilePrefab;
    [SerializeField] GameObject chunkPrefab;
    [SerializeField] ChunkTracker chunkTracker;
    [SerializeField] SpriteRenderer extendableBG;

    // These two could also be level-specific if necessary
    [SerializeField] float groundOffsetY;
    [SerializeField] float tileOffsetY;
    [SerializeField] float tileWidth;
    [SerializeField] float tileHeight;
    [SerializeField] float justOneMoreOffsetBroISwear;

    internal float bottomY;
    internal float topY;

    [SerializeField] float sizeFactor;
    [SerializeField] Vector2 playerStartPosition;
    [SerializeField] Color tileDisabledColor;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void LoadLevel()
    {
        int nrBreaks = 0;

        float notchSpacing = levelData.towerWidth / levelData.notchCount;

        GameObject startTile = Instantiate(startTilePrefab, tilesParent);
        startTile.transform.position = new Vector3(0, -tileOffsetY, 0);
        startTile.transform.localScale = new Vector3(levelData.levelWidth*2, 1, 1);
        //startTile.GetComponent<SpriteRenderer>().size = new Vector3(levelData.levelWidth + tileWidth, 1, 1);

        Chunk currChunk = Instantiate(chunkPrefab, new Vector2(0, .65f), Quaternion.identity, tilesParent).GetComponent<Chunk>();
        currChunk.Init(this, 0);
        List<Chunk> Chunks = new() { currChunk } ;

        for (int i = 0; i < levelData.tiles.Count; i++)
        {
            TileData tile = levelData.tiles[i];
            
            float posX = 0;
            int startNotch = 0;
            float offset = 0;

            if(!tile.isFixed)
            {
                int xCorrect = Random.Range(1, levelData.notchCount + 1) - 1;
                float posCorrectX = -levelData.towerWidth / 2 + xCorrect * notchSpacing;

                posX = posCorrectX;
                startNotch = xCorrect;

                offset = notchSpacing / 2;

                while (posX == posCorrectX)
                {
                    startNotch = Random.Range(1, levelData.notchCount + 1) - 1;
                    posX = -levelData.towerWidth / 2 + startNotch * notchSpacing;
                }
            }

            TonePlatform tileObj = Instantiate(tilePrefab, new Vector2(posX + offset, groundOffsetY + tileOffsetY * i + tileOffsetY * nrBreaks + justOneMoreOffsetBroISwear),
                    Quaternion.identity, tilesParent).GetComponent<TonePlatform>();

            tileObj.transform.localScale = new Vector3(tileWidth, tileHeight,  1);

            tileObj.Init(tile.isFixed, startNotch, levelData.notchCount, notchSpacing, FindFrequency(tile.correctFrequencyIdx, levelData.tuningSystem), levelData.centSpacing, tileDisabledColor);

            GameObject bgWall;
            if(tile.isFixed) bgWall = Instantiate(bgWallFixedPrefab, new Vector2(0, groundOffsetY + tileOffsetY * i + tileOffsetY * nrBreaks), Quaternion.identity, tilesParent);
            else bgWall = Instantiate(bgWallPrefab, new Vector2(0, groundOffsetY + tileOffsetY * i + tileOffsetY * nrBreaks), Quaternion.identity, tilesParent);

            currChunk.AppendPlatform(tileObj.gameObject);
            currChunk.AppendPlatform(bgWall);

            if (tile.endsChunk || tile.hasBreak)
            {
                float divideLine = tile.hasBreak ? groundOffsetY + tileOffsetY * (i + 1) + tileOffsetY * nrBreaks : //  touches bottom of break
                    groundOffsetY + tileOffsetY * (i+1) + tileOffsetY * nrBreaks; // halfway between this tile and next

                currChunk.FinishChunk(divideLine, tile.hasBreak); 

                currChunk = Instantiate(chunkPrefab, new Vector2(0, divideLine), Quaternion.identity, tilesParent).GetComponent<Chunk>();
                currChunk.Init(this, Chunks.Count);
                Chunks.Add(currChunk);
            }

            if (tile.hasBreak)
            {
                GameObject breakObj = Instantiate(breakTilePrefab, tilesParent);
                breakObj.transform.position = new Vector3(0, groundOffsetY + tileOffsetY * (i + 1) + tileOffsetY * nrBreaks + justOneMoreOffsetBroISwear/2, 0);
                breakObj.transform.localScale = new Vector3(levelData.levelWidth-4, 1, 1);
                breakObj.transform.GetChild(0).transform.localScale = new Vector3(1, sizeFactor, 1);

                bgWall = Instantiate(bgWallFixedPrefab, new Vector2(0, groundOffsetY + tileOffsetY * (i + 1) + tileOffsetY * nrBreaks), Quaternion.identity, tilesParent);
                currChunk.AppendPlatform(bgWall);

                nrBreaks++;
            }
        }

        bottomY = playerStartPosition.y;

        float totalHeight = groundOffsetY + tileOffsetY * (levelData.tiles.Count) + tileOffsetY * nrBreaks;

        GameObject wallLeft = Instantiate(wallPrefab, tilesParent);
        wallLeft.transform.position = new Vector3(-levelData.levelWidth / 2 - tileWidth*3/2, bottomY, 0);
        wallLeft.transform.GetChild(0).localScale = new Vector3(4, totalHeight * 2, 1);
        wallLeft.GetComponent<SpriteRenderer>().size = new Vector3(4, totalHeight * 2, 1);

        GameObject wallRight = Instantiate(wallPrefab, tilesParent);
        wallRight.transform.position = new Vector3(levelData.levelWidth / 2 + tileWidth * 3 / 2, bottomY, 0);
        wallRight.transform.GetChild(0).localScale = new Vector3(4, totalHeight * 2, 1);
        wallRight.GetComponent<SpriteRenderer>().size = new Vector3(4, totalHeight*2, 1);

        GameObject endTile = Instantiate(endTilePrefab, tilesParent);
        endTile.transform.position = new Vector3(0, totalHeight + justOneMoreOffsetBroISwear/2, 0);
        endTile.transform.localScale = new Vector3(levelData.levelWidth + tileWidth, 1, 1);
        endTile.transform.GetChild(0).transform.localScale = new Vector3(1, sizeFactor, 1);

        currChunk.AppendPlatform(Instantiate(bgWallEndPrefab, new Vector2(0, totalHeight), Quaternion.identity, tilesParent));

        extendableBG.size = new Vector2(extendableBG.size.x, totalHeight*2);

        currChunk.FinishChunk(totalHeight, true);

        topY = totalHeight + justOneMoreOffsetBroISwear;

        chunkTracker.CreateChunks(Chunks);

        if (PlayerManager.Instance)
            PlayerManager.Instance.controls.transform.position = new Vector3(playerStartPosition.x, playerStartPosition.y, 0);
        else FindFirstObjectByType<PlayerControls>().transform.position = new Vector3(playerStartPosition.x, playerStartPosition.y, 0);

    }

    public void LoadFromEditor()
    {
        ClearLoadedLevel();
        LoadLevel();
    }

    public void LoadFromManager(LevelData levelData)
    {
        Debug.Log("loading level " + levelData);
        this.levelData = levelData;

        ClearLoadedLevel();
        LoadLevel();
    }

    public void ClearLoadedLevel()
    {
        for (int i = tilesParent.childCount - 1; i >= 0; i--) DestroyImmediate(tilesParent.GetChild(i).gameObject);
    }

    public float FindFrequency(int n, int N, float refFrequency=440f) {
        // ref note is A4; pos is relative in each system
        int aPos = N == 12 ? 21 : N == 5 ? 14 : 33;
        return refFrequency * Mathf.Pow(2f, (n - aPos) * 1.0f / N);
    }
}

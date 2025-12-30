using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData levelData;

    [SerializeField] GameObject tilePrefab;
    [SerializeField] Transform tilesParent;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject startTilePrefab;
    [SerializeField] GameObject endTilePrefab;
    [SerializeField] GameObject breakTilePrefab;

    // These two could also be level-specifici if necessary
    [SerializeField] float groundOffsetY;
    [SerializeField] float tileOffsetY;
    [SerializeField] float tileWidth;

    public void LoadLevel()
    {
        int nrBreaks = 0;

        float notchSpacing = levelData.levelWidth / levelData.notchCount;
        float offset = notchSpacing / 2;

        for (int i = 0; i < levelData.tiles.Count; i++)
        {
            TileData tile = levelData.tiles[i];

            int xCorrect = Random.Range(1, levelData.notchCount + 1) - 1;
            float posCorrectX = -levelData.levelWidth/2 + xCorrect * notchSpacing;

            float posX = posCorrectX;
            int startNotch = xCorrect;

            if (!tile.isFixed) 
            {
                while (posX == posCorrectX)
                {
                    startNotch = Random.Range(1, levelData.notchCount + 1) - 1;
                    posX = -levelData.levelWidth / 2 + startNotch * notchSpacing;
                }
            }

            //Debug.Log("tile " + i + " fixed: " + tile.isFixed + " x correct " + xCorrect + " pos correct " + posCorrectX + " pos final " + posX);

            Vector3 pos = new Vector3(posX + offset, groundOffsetY + tileOffsetY * i + tileOffsetY * nrBreaks, 0);

            GameObject obj = Instantiate(tilePrefab, tilesParent);
            obj.transform.position = pos;

            obj.GetComponent<TonePlatform>().Init(tile.isFixed, startNotch, levelData.notchCount, notchSpacing, tile.correctFrequency, levelData.centSpacing);

            if (tile.hasBreak)
            {
                GameObject breakObj = Instantiate(breakTilePrefab, tilesParent);
                breakObj.transform.position = new Vector3(0, groundOffsetY + tileOffsetY * (i + 1) + tileOffsetY * nrBreaks, 0);
                breakObj.transform.localScale = new Vector3(levelData.levelWidth + tileWidth, 1, 1);
                nrBreaks++;
            }
        }

        float totalHeight = groundOffsetY + tileOffsetY * (levelData.tiles.Count) + tileOffsetY * nrBreaks;

        GameObject wallLeft = Instantiate(wallPrefab, tilesParent);
        wallLeft.transform.position = new Vector3(-levelData.levelWidth / 2 - tileWidth / 2, totalHeight / 2, 0);
        wallLeft.transform.localScale = new Vector3(1, totalHeight, 1);

        GameObject wallRight = Instantiate(wallPrefab, tilesParent);
        wallRight.transform.position = new Vector3(levelData.levelWidth / 2 + tileWidth / 2, totalHeight / 2, 0);
        wallRight.transform.localScale = new Vector3(1, totalHeight, 1);

        GameObject startTile = Instantiate(startTilePrefab, tilesParent);
        startTile.transform.position = new Vector3(0, 0, 0);
        startTile.transform.localScale = new Vector3(levelData.levelWidth + tileWidth, 1, 1);

        GameObject endTile = Instantiate(endTilePrefab, tilesParent);
        endTile.transform.position = new Vector3(0, totalHeight, 0);
        endTile.transform.localScale = new Vector3(levelData.levelWidth + tileWidth, 1, 1);

        FindFirstObjectByType<PlayerControls>().transform.position = new Vector3(levelData.playerStartPosition.x, levelData.playerStartPosition.y, 0);
    }

    public void LoadFromEditor()
    {
        for (int i = tilesParent.childCount - 1; i >= 0; i--) DestroyImmediate(tilesParent.GetChild(i).gameObject);
        LoadLevel();
    }

    public void Start()
    {
        for (int i = tilesParent.childCount - 1; i >= 0; i--) Destroy(tilesParent.GetChild(i).gameObject);
        LoadLevel();
    }
}

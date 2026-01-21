using System.Collections;
using UnityEngine;

public class FinalScoreCounter : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] ScoreConversions _Conversions;

    [Header("Anim Valls")]
    [SerializeField] float MoveSpeed;

    private void OnEnable()
    {
        GameManager.EndGame += OnEndGame;
    }

    private void OnDisable()
    {
        GameManager.EndGame -= OnEndGame;
    }

    int rankGotten = 0;
    int scoreGotten = 0;

    void OnEndGame() 
    {
        scoreGotten = ChunkTracker.Instance.TotalScore();

        int bestPossibleScore = 0;
        foreach (var plat in GameManager.Instance.levels[GameManager.Instance.selectedLevel].tiles)
        {
            if (!plat.isFixed) bestPossibleScore += _Conversions.ErrorToScore[0].Score;
        }
        foreach (var i in _Conversions.ScorePercentToRank)
        {
            if ((float)scoreGotten / bestPossibleScore >= i.ScorePercentThreshold)
            {
                rankGotten = i.Rank;
            }
        }
        GameManager.Instance.SaveManager.CompleteLevel(rankGotten, scoreGotten, ChunkTracker.Instance.LevelTimer);
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        transform.position = new Vector2(0, LevelManager.Instance.bottomY);

        // yield return wait until camera reaches target point

        Chunk prevChunk = null;
        while (transform.position.y < LevelManager.Instance.topY)
        {
            Chunk hoveredChunk = ChunkTracker.Instance.GetChunkByYPos(transform.position.y);
            if (hoveredChunk != prevChunk && hoveredChunk != null)
            {
                hoveredChunk.PlayChunkTones(true);
            }
            prevChunk = hoveredChunk;
            yield return null;
            transform.position = new Vector2(0, transform.position.y + MoveSpeed * Time.deltaTime);
        }
    }
}
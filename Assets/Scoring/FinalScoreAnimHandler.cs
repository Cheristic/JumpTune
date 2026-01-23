using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class FinalScoreCounter : MonoBehaviour
{
    public static event Action CompletedEndSequence;
    [Header("Links")]
    [SerializeField] ScoreConversions _Conversions;
    [SerializeField] CinemachineCamera _cam;
    [SerializeField] GameObject EndScoreHolder;
    [SerializeField] TMP_Text _Rank;
    [SerializeField] TMP_Text _Score;
    [SerializeField] TMP_Text _Time;


    [Header("Anim Valls")]
    [SerializeField] float MoveSpeed;

    private void OnEnable()
    {
        GameManager.EndGame += OnEndGame;
        EndScoreHolder.SetActive(false);
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
        Debug.Log(scoreGotten + " " + bestPossibleScore + " " + 1.0f * scoreGotten / bestPossibleScore);
        foreach (var i in _Conversions.ScorePercentToRank)
        {
            if (1.0f * scoreGotten / bestPossibleScore >= i.ScorePercentThreshold)
            {
                rankGotten = i.Rank;
                break;
            }
        }
        Debug.Log(rankGotten + " " + _Conversions.GetRankTextFromRank(rankGotten));
        GameManager.Instance.SaveManager.CompleteLevel(rankGotten, scoreGotten, ChunkTracker.Instance.LevelTimer);
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        transform.position = new Vector2(0, LevelManager.Instance.bottomY);

        _cam.Follow = transform;

        yield return new WaitUntil(() => _cam.transform.position.y < 2f);

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

        _Rank.text = _Conversions.GetRankTextFromRank(rankGotten);
        _Score.text = scoreGotten.ToString();
        TimeSpan t = TimeSpan.FromSeconds(ChunkTracker.Instance.LevelTimer);
        _Time.text = t.ToString("mm':'ss'.'ff");
        EndScoreHolder.SetActive(true);
    }

    public void ContinueToMainMenu()
    {
        GameManager.Instance.SwapToMainMenu();
    }
}
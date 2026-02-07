using System.Collections.Generic;
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
    [SerializeField] TMP_Text _Cleared;


    [Header("Anim Valls")]
    [SerializeField] float ClearDelayTime = .6f;
    [SerializeField] float MoveSpeed;
    [SerializeField] float CheckYOffset;

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
        //Debug.Log(scoreGotten + " " + bestPossibleScore + " " + 1.0f * scoreGotten / bestPossibleScore);
        foreach (var i in _Conversions.ScorePercentToRank)
        {
            if (1.0f * scoreGotten / bestPossibleScore >= i.ScorePercentThreshold)
            {
                rankGotten = i.Rank;
                break;
            }
        }
        GameManager.Instance.SaveManager.CompleteLevel(rankGotten, scoreGotten, ChunkTracker.Instance.LevelTimer);
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(ClearDelayTime);
        PlayerManager.Instance.controls.transform.position = new Vector2(0, LevelManager.Instance.topY + 4f);
        transform.position = new Vector2(0, LevelManager.Instance.bottomY);

        _cam.transform.position = new Vector2(0, LevelManager.Instance.bottomY);
        _cam.Follow = transform;

        yield return new WaitUntil(() => _cam.transform.position.y < 2f);

        // yield return wait until camera reaches target point

        List<TonePlatform> platforms = ChunkTracker.Instance.GetAllPlatforms();
        int currPlat = 0;
        while (transform.position.y < LevelManager.Instance.topY)
        {
            if (currPlat < platforms.Count)
            {
                if (platforms[currPlat].transform.position.y < transform.position.y + CheckYOffset)
                {
                    if (!platforms[currPlat].isFixed) platforms[currPlat].ShowError();
                    platforms[currPlat].PlayPlatformTone();
                    currPlat++;

                }
            }
            yield return null;
            transform.position = new Vector2(0, transform.position.y + MoveSpeed * Time.deltaTime);
        }

        _Rank.text = _Conversions.GetRankTextFromRank(rankGotten);
        _Score.text = scoreGotten.ToString();
        TimeSpan t = TimeSpan.FromSeconds(ChunkTracker.Instance.LevelTimer);
        _Time.text = t.ToString("mm':'ss'.'ff");
        EndScoreHolder.SetActive(true);
        _Cleared.text = rankGotten == 1 ? "Perfect!" : rankGotten <= 6 ? "Nice job!" : "Try again!";
        ToneManager.Instance.sfxPlayer.PlayStartSound(-.25f);
    }

    public void ContinueToMainMenu()
    {
        GameManager.Instance.SwapToMainMenu();
    }
}
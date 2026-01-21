using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Conversions", menuName = "Game/ScoreConversions")]
public class ScoreConversions : ScriptableObject
{
    [Serializable]
    public class ErrorToScoreEntry
    {
        public int MaxError;
        public int Score;
        public Color Color;
    }
    public List<ErrorToScoreEntry> ErrorToScore;
    [Serializable]
    public class ScoreToRankEntry
    {
        public float ScorePercentThreshold;
        public int Rank;
        public string RankText;
    }
    public List<ScoreToRankEntry> ScorePercentToRank;

    public string GetRankTextFromRank(int rankVal)
    {
        foreach (var s in ScorePercentToRank) if (rankVal == s.Rank)
            {
                return s.RankText;
            }
        return "";
    }
    public int ScoreFromError(int error)
    {
        foreach (var i in ErrorToScore)
        {
            if (error <= i.MaxError)
            {
                return i.Score;
            }
        }
        return 0;
    }
}

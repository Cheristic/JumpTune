using System.Collections;
using UnityEngine;
using TMPro;
public class TonePlatformErrorAnim : MonoBehaviour
{
    [SerializeField] ScoreConversions _Conversions;
    [SerializeField] Canvas canvas;
    [SerializeField] TMP_Text text;
    [SerializeField] AnimationCurve MoveCurve;
    [SerializeField] float StartPos;
    [SerializeField] float EndPos;
    [SerializeField] AnimationCurve OpacityCurve;
    [SerializeField] float AnimTime;

    public void Start()
    {
        canvas.worldCamera = Camera.main;
        text.alpha = 0;
    }

    float timeProgressed;
    public IEnumerator ErrorAnim(int score)
    {
        foreach (var i in _Conversions.ErrorToScore)
        {
            if (score == i.Score)
            {
                text.text = i.Score.ToString();
                text.color = i.Color;
            }
        }

        while (timeProgressed < AnimTime)
        {
            float prog = timeProgressed / AnimTime;
            text.alpha = OpacityCurve.Evaluate(prog);
            text.transform.localPosition = new Vector2(0, Mathf.Lerp(StartPos, EndPos, MoveCurve.Evaluate(prog)));
            timeProgressed += Time.deltaTime;
            yield return null;
        }
        text.alpha = 0.0f;
        yield return null;
    }
}
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
public class LevelSelectMenu : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] CanvasGroup canvas;
    [SerializeField] TMP_Text _Rank;
    [SerializeField] TMP_Text _Score;
    [SerializeField] TMP_Text _Time;
    [SerializeField] Transform ButtonsHolder;
    [SerializeField] ScoreConversions _Conversions;
    [SerializeField] TMP_Text _Title;
    [SerializeField] TMP_Text _CentDifference;
    [SerializeField] TMP_Text _Notches;

    [Header("Anim Vals")]
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float AnimTime;

    List<RectTransform> buttons;

    private void OnEnable()
    {
        if (buttons == null)
        {
            buttons = new List<RectTransform>();
            for (int i = 0; i < ButtonsHolder.childCount; i++)
            {
                buttons.Add(ButtonsHolder.GetChild(i).GetComponent<RectTransform>());
                ButtonsHolder.GetChild(i).GetComponent<Button>().interactable = GameManager.Instance.SaveManager.CurrData.levels[i].isUnlocked;
            }
        }
        canvas.alpha = 0;
        buttonHovering = -1;
        StartCoroutine(CheckForHover());
    }

    public void SwapToLevel(int level) => GameManager.Instance.SwapToLevelPreview(level);

    int buttonHovering;
    IEnumerator CheckForHover()
    {
        bool inBounds(int b)
        {
            var but = buttons[b];
            return Input.mousePosition.x > (but.position.x- but.sizeDelta.x/2) &&
                    Input.mousePosition.x < (but.position.x + but.sizeDelta.x/2) &&
                    Input.mousePosition.y > (but.position.y - but.sizeDelta.y/2) &&
                    Input.mousePosition.y < (but.position.y + but.sizeDelta.y/2);
        }

        while (true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            for (int i = 0; i <  buttons.Count; i++) 
            {
                // AABB detection
                if (inBounds(i))
                {
                    buttonHovering = i;
                    break;
                }
            }

            if (buttonHovering == -1) { yield return null; continue; }

            if (IFadeAnim != null) StopCoroutine(IFadeAnim);
            StartCoroutine(IFadeAnim = FadeAnim(true));
            DisplayButton();
            yield return null;

            while (inBounds(buttonHovering))
            {
                yield return null;
            }

            if (IFadeAnim != null) StopCoroutine(IFadeAnim);
            StartCoroutine(IFadeAnim = FadeAnim(false));
            buttonHovering = -1;
        }
    }

    IEnumerator IFadeAnim;
    float timeProgressed = 0;
    IEnumerator FadeAnim(bool fadeIn)
    {        
        while (timeProgressed >= 0 && timeProgressed <= AnimTime)
        {
            float prog = timeProgressed / AnimTime;
            canvas.alpha = FadeCurve.Evaluate(prog);
            timeProgressed = fadeIn ? timeProgressed + Time.deltaTime : timeProgressed - Time.deltaTime;
            yield return null;
        }

        timeProgressed = fadeIn ? AnimTime : 0;
    }

    void DisplayButton()
    {
        var data = GameManager.Instance.SaveManager.CurrData.levels[buttonHovering];

        if (data.rank == 0) // empty
        {
            _Rank.text = "-";
            _Score.text = "-";
            _Time.text = "-";
        } else
        {
            _Rank.text = _Conversions.GetRankTextFromRank(data.rank);
            _Score.text = data.score.ToString();
            TimeSpan t = TimeSpan.FromSeconds(data.time);
            _Time.text = t.ToString("mm':'ss'.'ff");
        }

        int tuning = GameManager.Instance.levels[buttonHovering].tuningSystem;
        _Title.text = tuning switch
        {
            5 => "5-Tone Equal Temperament",
            12 => "12-Tone Equal Temperament",
            _ => "19-Tone Equal Temperament"
        };
        _CentDifference.text = "Cent Interval: " + GameManager.Instance.levels[buttonHovering].centSpacing.ToString("0");
        _Notches.text = "Notches: " + GameManager.Instance.levels[buttonHovering].notchCount.ToString();
    }
}

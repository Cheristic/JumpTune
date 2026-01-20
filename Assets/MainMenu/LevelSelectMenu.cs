using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public class LevelSelectMenu : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] CanvasGroup canvas;
    [SerializeField] TMP_Text _Rank;
    [SerializeField] TMP_Text _Score;
    [SerializeField] TMP_Text _Time;
    [SerializeField] Transform ButtonsHolder;

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
            }
        }
        canvas.alpha = 0;
        buttonHovering = -1;
        StartCoroutine(CheckForHover());
    }

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
            Debug.Log(Input.mousePosition);
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
            yield return null;

            while (inBounds(buttonHovering))
            {
                Debug.Log("Showing " + buttonHovering);
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
}

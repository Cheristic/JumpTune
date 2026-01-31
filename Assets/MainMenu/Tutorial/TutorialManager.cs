using System.Collections;
using UnityEngine;
using System;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] GameObject[] Slides;
    [SerializeField] GameObject LeftArrow;
    [SerializeField] GameObject RightArrow;

    [Header("Slide Specific")]
    [SerializeField] Transform Slide4_Mover;
    [SerializeField] Vector2 AutoMoveInterval;
    [SerializeField] float LeftFrequency;
    [SerializeField] float Interval;
    [SerializeField] int NotchCount;
    [SerializeField] float MoveAmount;

    [Header("Keys")]
    [SerializeField] GameObject J_Key;
    [SerializeField] GameObject L_Key;


    int currSlide = 0;
    internal PlayerInput input;

    bool hasBeenInitialized = false;
    void Init()
    {
        if (hasBeenInitialized) return;
        input = new();
    }

    private void OnEnable()
    {
        Init();
        currSlide = 0;
        for (int i = 0; i < Slides.Length; i++) Slides[i].SetActive(i == currSlide);
        Navigate(0);
    }
    private void OnDisable()
    {
        input.Disable();
    }

    public void Navigate(int dir)
    {
        if (dir < 0 && currSlide == 0 || dir > 0 && currSlide == Slides.Length - 1) return;

        Slides[currSlide].SetActive(false);
        currSlide += dir;
        Slides[currSlide].SetActive(true);

        LeftArrow.SetActive(currSlide != 0);
        RightArrow.SetActive(currSlide != Slides.Length-1);

        if (currSlide == 4) StartCoroutine(Slide4());
    }

    IEnumerator Slide4()
    {
        input.Enable();
        yield return null;
        int currNotch = 2;
        int lastDir = 0;
        J_Key.SetActive(true);
        L_Key.SetActive(true);
        Slide4_Mover.localPosition = new Vector2(0, Slide4_Mover.localPosition.y);

        void DisableKey(int dir)
        {
            if (dir == -1) J_Key.SetActive(false);
            else if (dir == 1) L_Key.SetActive(false);
        }
        while (currSlide == 4)
        {
            // Auto Move Loop
            while (currSlide == 4)
            {
                float t = 0;
                float waitT = UnityEngine.Random.Range(AutoMoveInterval.x, AutoMoveInterval.y);
                while (t < waitT && lastDir == 0)
                {
                    yield return null;
                    t += Time.deltaTime;
                    lastDir = Math.Sign(input.Player.MoveTone.ReadValue<float>());
                }

                if (currSlide != 4) break;

                if (lastDir != 0)
                {
                    currNotch += lastDir;
                    DisableKey(lastDir);
                    Slide4_Mover.position = new Vector2(Slide4_Mover.position.x + lastDir * MoveAmount, Slide4_Mover.position.y);
                    ToneManager.Instance.PlayNote(LeftFrequency * Mathf.Pow(2, Interval * currNotch / 1200.0f));
                    break;
                }

                int dir = currNotch == 0 ? 1 : currNotch == NotchCount - 1 ? -1 : UnityEngine.Random.Range(0, 2) * 2 - 1;
                currNotch += dir;
                Slide4_Mover.position = new Vector2(Slide4_Mover.position.x + dir * MoveAmount, Slide4_Mover.position.y);
                ToneManager.Instance.PlayNote(LeftFrequency * Mathf.Pow(2, Interval * currNotch / 1200.0f));
            }
            // Manual Move Loop
            while (currSlide == 4)
            {
                int dir = Math.Sign(input.Player.MoveTone.ReadValue<float>());
                if (dir != lastDir && (dir == -1 && currNotch > 0 || dir == 1 && currNotch < NotchCount - 1))
                {
                    DisableKey(dir);
                    currNotch += dir;
                    Slide4_Mover.position = new Vector2(Slide4_Mover.position.x + dir * MoveAmount, Slide4_Mover.position.y);
                    ToneManager.Instance.PlayNote(LeftFrequency * Mathf.Pow(2, Interval * currNotch / 1200.0f));
                }
                lastDir = dir;
                yield return null;
            }
        }
        input.Disable();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public LevelData[] previewLevels;
    [SerializeField] RectTransform continueButton;
    [SerializeField] TMP_Text continueButtonText;
    [SerializeField] float CONTINUE_Width;
    [SerializeField] float SKIP_Width;
    [SerializeField] TMP_Text DEMO_TEXT;
    [SerializeField] float demoFlashInterval;
    [SerializeField] TMP_Text FREQUENCY_Text;
    [SerializeField] GameObject OctaveHolder;
    [SerializeField] TMP_Text OctaveCount;

    [SerializeField] float initialWaitPeriod = 1.0f;
    [SerializeField] LayerMask PlatformLayerMask;

    bool isDemoing = true;
    Chunk PlatformChunk;
    List<TonePlatform> Platforms;
    void Start()
    {
        if (LevelManager.Instance != null)
        {

            List<Chunk> chunks = LevelManager.Instance.LoadFromManager(GetLevelData());
            PlatformChunk = chunks[0]; // should only be one
        }

        Platforms = PlatformChunk.PrepForPreviewScene();
        OctaveHolder.SetActive(false);
        FREQUENCY_Text.text = "";
        continueButtonText.text = "SKIP";
        OctaveCount.text = "0";
        continueButton.sizeDelta = new Vector2(SKIP_Width, continueButton.sizeDelta.y);

        StartCoroutine(PreviewScene());
        StartCoroutine(FlashDemo());
    }

    LevelData GetLevelData() => TuningSystem switch
    {
        12 => previewLevels[0],
        5 => previewLevels[1],
        _ => previewLevels[2] // 19-TET
    };

    IEnumerator PreviewScene()
    {
        yield return new WaitForSeconds(initialWaitPeriod);
        yield return PlayChunkTones(false);
        yield return new WaitForSeconds(initialWaitPeriod);
        yield return PlayChunkTones(true);
        yield return new WaitForSeconds(initialWaitPeriod);

        continueButtonText.text = "CONTINUE";
        continueButton.sizeDelta = new Vector2(CONTINUE_Width, continueButton.sizeDelta.y);
        isDemoing = false;
        DEMO_TEXT.gameObject.SetActive(false);
        OctaveHolder.SetActive(true);
        StartCoroutine(CheckForHover());
    }

    public Coroutine PlayChunkTones(bool reverse = false)
    {
        StopAllCoroutines();
        return StartCoroutine(Player());

        IEnumerator Player()
        {

            int i = reverse ? PlatformChunk.platforms.Count - 1 : 0;
            for (; i < PlatformChunk.platforms.Count && i >= 0;)
            {
                if (PlatformChunk.platforms[i].TryGetComponent<TonePlatform>(out var tp))
                {
                    tp.PlayPlatformTone();
                    tp.SetOutline();
                    if (tp.hasPlayer) yield break;
                    yield return new WaitForSeconds(PlatformChunk.timeBetweenNotes);
                    tp.SetNoOutline();
                }

                if (reverse) i--;
                else i++;
            }
        }
    }

    IEnumerator CheckForHover()
    {
        TonePlatform lastTP = null;
        while (true)
        {
            bool wasSet = false;
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, .001f, PlatformLayerMask);
            //Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.down*.001f, Color.green, .1f);
            if (hit.collider != null)
            {
                if (hit.collider.TryGetComponent<TonePlatform>(out var tp))
                {
                    if (lastTP != tp)
                    {
                        if (lastTP != null) lastTP.SetNoOutline();
                        tp.PlayPlatformTone();
                        tp.SetOutline();
                        lastTP = tp;
                        FREQUENCY_Text.text = tp.leftMostFrequency.ToString("0.00") + " Hz";
                    }
                    wasSet = true;
                }
            }
            if (!wasSet)
            {
                FREQUENCY_Text.text = "";
                if (lastTP != null)
                {
                    lastTP.SetNoOutline();
                    lastTP = null;
                }
            }
            yield return null;
        }
    }

    IEnumerator FlashDemo()
    {
        DEMO_TEXT.gameObject.SetActive(true);
        yield return new WaitForSeconds(demoFlashInterval);
        while (isDemoing)
        {
            DEMO_TEXT.gameObject.SetActive(!DEMO_TEXT.gameObject.activeInHierarchy);
            yield return new WaitForSeconds(demoFlashInterval);
        }
    }

    public void Continue()
    {
        if (isDemoing)
        {
            StopAllCoroutines();
            continueButtonText.text = "CONTINUE";
            continueButton.sizeDelta = new Vector2(CONTINUE_Width, continueButton.sizeDelta.y);
            isDemoing = false;
            DEMO_TEXT.gameObject.SetActive(false);
            OctaveHolder.SetActive(true);
            foreach (var plat in Platforms) plat.SetNoOutline();
            StartCoroutine(CheckForHover());
        } else
        {
            GameManager.Instance.SwapToLevel();
        }
    }

    int currOctave = 0;
    public void ShiftOctave(int dir)
    {
        if (TuningSystem == 5) if (currOctave + dir <= -2 || currOctave + dir >= 1) return;
        if (currOctave + dir <= -3 || currOctave + dir >= 2) return;

        currOctave += dir;
        OctaveCount.text = currOctave.ToString();

        float FindFrequency(int n, float refFrequency = 440f)
        {
            // ref note is A4; pos is relative in each system
            int aPos = TuningSystem == 12 ? 21 : TuningSystem == 5 ? 14 : 33;
            return refFrequency * Mathf.Pow(2f, (n - aPos) * 1.0f / TuningSystem);
        }
        for (int i = 0; i < GetLevelData().tiles.Count; i++)
        {
            TileData tile = GetLevelData().tiles[i];
            TonePlatform tp = Platforms[i];
            int adder = TuningSystem == 5 ? 10 : TuningSystem;
            tp.leftMostFrequency = FindFrequency(tile.correctFrequencyIdx + adder * currOctave);
        }
    }

    int TuningSystem
    {
        get => GameManager.Instance.levels[GameManager.Instance.selectedLevel].tuningSystem;
    }
}

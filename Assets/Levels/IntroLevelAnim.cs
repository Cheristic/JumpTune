using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;

public class IntroLevelAnim : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] CinemachineCamera _cam;
    [SerializeField] TMP_Text PreviewText;
    [SerializeField] Transform StartText;
    [SerializeField] GameObject SkipButton;

    [Header("Anim Valls")]
    [SerializeField] float MoveSpeed;
    [SerializeField] float DownAcceleration;
    [SerializeField] float DownMoveSpeed;
    [SerializeField] float CheckYOffset;
    [SerializeField] float PayAttentionInterval;
    [SerializeField] int PayAttentionBlinkTimes;
    [SerializeField] float ScaleSize;
    [SerializeField] float ScaleInTime;
    [SerializeField] float ScaleHoldTime;
    [SerializeField] float ScaleOutTime;

    float startYPosCam = 0;

    private void Start()
    {
        startYPosCam = _cam.transform.localPosition.y;
        _cam.Follow = transform;
        StartText.localScale = Vector3.zero;
        StartText.gameObject.SetActive(false);
        SkipButton.gameObject.SetActive(true);
        StartCoroutine(StartSequence());
        DOTween.Init();
    }

    public void Skip()
    {
        StopAllCoroutines();
        SkipButton.gameObject.SetActive(false);
        GameManager.Instance.TriggerStartGame();

        _cam.Follow = PlayerManager.Instance.transform;
        _cam.transform.position = new Vector2(0, LevelManager.Instance.bottomY);

        PreviewText.alpha = 0;
        StartText.gameObject.SetActive(true);
        var seq = DOTween.Sequence();
        seq.Append(StartText.DOScale(new Vector3(ScaleSize, ScaleSize, ScaleSize), ScaleInTime)
            .SetEase(Ease.OutBounce));
        seq.Append(StartText.DOScale(Vector3.zero, ScaleOutTime)
            .SetEase(Ease.InCubic).SetDelay(ScaleHoldTime));
        seq.OnComplete(() => StartText.gameObject.SetActive(false));
    }

    IEnumerator StartSequence()
    {
        transform.position = new Vector2(0, LevelManager.Instance.bottomY);
        _cam.transform.position = new Vector2(0, LevelManager.Instance.bottomY);

        int blinks = 0;
        while (blinks < PayAttentionBlinkTimes)
        {
            blinks++;
            PreviewText.alpha = 1;
            yield return new WaitForSeconds(PayAttentionInterval);
            PreviewText.alpha = 0;
            if (blinks == PayAttentionBlinkTimes) break;
            yield return new WaitForSeconds(PayAttentionInterval);
        }

        List<TonePlatform> platforms = ChunkTracker.Instance.GetAllPlatforms();
        int currPlat = 0;
        while (transform.position.y < LevelManager.Instance.topY)
        {
            if (currPlat < platforms.Count)
            {
                if (platforms[currPlat].transform.position.y < transform.position.y + CheckYOffset)
                {
                    platforms[currPlat].PlayPlatformTone(true);
                    currPlat++;
                }
            }
            yield return null;
            transform.position = new Vector2(0, transform.position.y + MoveSpeed * Time.deltaTime);
        }

        float speed = DownAcceleration * Time.deltaTime;
        while (transform.position.y > PlayerManager.Instance.transform.position.y)
        {
            yield return null;
            transform.position = new Vector2(0, transform.position.y - speed * Time.deltaTime);
            speed = Mathf.Clamp(speed + DownAcceleration * Time.deltaTime, 0, DownMoveSpeed);
        }

        _cam.Follow = PlayerManager.Instance.transform;

        StartText.gameObject.SetActive(true);
        SkipButton.gameObject.SetActive(false);


        GameManager.Instance.TriggerStartGame();

        var seq = DOTween.Sequence();
        seq.Append(StartText.DOScale(new Vector3(ScaleSize, ScaleSize, ScaleSize), ScaleInTime)
            .SetEase(Ease.OutBounce));
        seq.Append(StartText.DOScale(Vector3.zero, ScaleOutTime)
            .SetEase(Ease.InCubic).SetDelay(ScaleHoldTime));
        seq.OnComplete(() => StartText.gameObject.SetActive(false));
    }
}

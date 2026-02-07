using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class TutorialPlayerManager : MonoBehaviour
{
    public PlayerControls controls;
    [SerializeField] GameObject W_Key;
    [SerializeField] GameObject S_Key;
    [SerializeField] GameObject K_Key;
    [SerializeField] float Platform1Frequency;
    [SerializeField] float Platform2Frequency;
    [SerializeField] float Platform1YThreshold;
    [SerializeField] float Platform0YThreshold;
    [SerializeField] float StrumIntervalTime;

    [Header("Moving Platform")]
    [SerializeField] Transform Platform0;
    [SerializeField] float Platform0Frequency;
    [SerializeField] float Interval;
    [SerializeField] int NotchCount;
    [SerializeField] float MoveAmount;

    Vector2 baseWKey;
    Vector2 baseSKey;
    Vector2 basePlayerPos;
    float LeftFrequency = 0;
    private void Awake()
    {
        controls.Init();
        controls.EnableInput();
        baseWKey = W_Key.transform.position;
        baseSKey = S_Key.transform.position;
        basePlayerPos = transform.position;
    }

    private void OnEnable()
    {
        hasLandedBefore = false;
        controls.EnableInput();
        controls.input.Player.Jump.started += Jump;
        W_Key.SetActive(true);
        S_Key.SetActive(false);
        K_Key.SetActive(false);
        Platform0.localPosition = new Vector2(0, Platform0.localPosition.y);
        LeftFrequency = Platform0Frequency * Mathf.Pow(2, -Interval * (NotchCount/2) / 1200.0f);
        StartCoroutine(HandleTones());
    }

    void Jump(InputAction.CallbackContext ctx)
    {
        W_Key.SetActive(false);
        controls.input.Player.Jump.started -= Jump;
    }
    void Fall(InputAction.CallbackContext ctx)
    {
        S_Key.SetActive(false);
        controls.input.Player.Drop.started -= Fall;
    }
    void Strum(InputAction.CallbackContext ctx)
    {
        K_Key.SetActive(false);
        if (IStrum != null) StopCoroutine(IStrum);
        StartCoroutine(IStrum = Strum());
    }
    IEnumerator IStrum;
    IEnumerator Strum()
    {
        ToneManager.Instance.PlayNote(Platform2Frequency);

        if (transform.localPosition.y <= Platform1YThreshold) yield break;
        yield return new WaitForSeconds(StrumIntervalTime);
        ToneManager.Instance.PlayNote(Platform1Frequency);

        if (transform.localPosition.y <= Platform0YThreshold) yield break;
        yield return new WaitForSeconds(StrumIntervalTime);
        ToneManager.Instance.PlayNote(LeftFrequency * Mathf.Pow(2, Interval * currNotch / 1200.0f));
    }
    private void OnDisable()
    {
        controls.input.Player.Jump.started -= Jump;
        controls.input.Player.Drop.started -= Fall;
        controls.input.Player.StrumChunk.started -= Strum;
    }

    private void Update()
    {
        Vector2 diff = (Vector2)transform.position - basePlayerPos;
        W_Key.transform.position = baseWKey + diff;
        S_Key.transform.position = baseSKey + diff;
    }

    bool hasLandedBefore = false;
    int currNotch = 0;
    IEnumerator HandleTones()
    {
        currNotch = NotchCount / 2 + 2;
        Platform0.position = new Vector2(Platform0.position.x + 2 * MoveAmount, Platform0.position.y);
        yield return new WaitForSeconds(.1f);
        while (true)
        {
            yield return new WaitUntil(() => !controls.isGrounded);
            yield return new WaitUntil(() => controls.isGrounded);
            if (!hasLandedBefore)
            {
                hasLandedBefore = true;
                S_Key.SetActive(true);
                controls.input.Player.Drop.started += Fall;
                K_Key.SetActive(true);
                controls.input.Player.StrumChunk.started += Strum;
            }

            if (transform.localPosition.y > Platform0YThreshold)
            {
                int lastDir = 0;
                ToneManager.Instance.PlayNote(LeftFrequency * Mathf.Pow(2, Interval * currNotch / 1200.0f));
                while (controls.isGrounded)
                {
                    int dir = Math.Sign(controls.input.Player.MoveTone.ReadValue<float>());
                    if (dir != lastDir && (dir == -1 && currNotch > 0 || dir == 1 && currNotch < NotchCount - 1))
                    {
                        currNotch += dir;
                        Platform0.position = new Vector2(Platform0.position.x + dir * MoveAmount, Platform0.position.y);
                        transform.position = new Vector2(transform.position.x + dir * MoveAmount, transform.position.y);
                        ToneManager.Instance.PlayNote(LeftFrequency * Mathf.Pow(2, Interval * currNotch / 1200.0f));
                    }
                    lastDir = dir;
                    yield return null;
                }
            } else if (transform.localPosition.y > Platform1YThreshold)
            {
                ToneManager.Instance.PlayNote(Platform1Frequency);
            } else
            {
                ToneManager.Instance.PlayNote(Platform2Frequency);
            }           
        }
    }
}

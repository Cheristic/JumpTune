using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TonePlatform : MonoBehaviour
{
    internal int notchCount;
    internal float notchSpacingInWorldCoords;

    [Header("Movement")]
    [SerializeField] float InitialHoldLagTime;
    [SerializeField] float HoldLagSpeedUp;
    [SerializeField] float MinLagTime;
    [SerializeField] float HoldLagSlowDown;

    [Header("Tone")]
    [SerializeField] float FrequencyMult;

    internal int currNotch;
    internal bool isFixed;

    public void Init(bool _Fixed, int _StartingNotch, int _NotchCount, float _NotchSpacingWorld) 
    { 
        currNotch = _StartingNotch;
        isFixed = _Fixed;
        notchCount = _NotchCount;
        notchSpacingInWorldCoords = _NotchSpacingWorld;

        if(isFixed)
        {
            TonePlatformTrigger tonePlatformTrigger = this.transform.GetComponentInChildren<TonePlatformTrigger>();
            tonePlatformTrigger.gameObject.SetActive(false);

            foreach (SpriteRenderer s in this.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                s.color = new Color(s.color.r - 0.2f, s.color.g - 0.2f, s.color.b - 0.2f, 1f);
            }
        }
    }

    private void OnDisable() => DisableMovement();

    public void EnableMovement()
    {
        StartCoroutine(HandleMoveInput());
    }
    public void DisableMovement()
    {
        StopAllCoroutines();
    }

    IEnumerator HandleMoveInput()
    { 
        yield return new WaitUntil(() => PlayerManager.Instance.controls.isGrounded);

        float currLagTime = InitialHoldLagTime;
        while (true)
        {
            int dir = Math.Sign(PlayerManager.Instance.Input.Player.MoveTone.ReadValue<float>());
            if ( dir != 0 && ((dir < 0 && currNotch > 0) || (dir > 0 && currNotch < NotchCount - 1)))
            {
                int claimedActiveNote = ToneManager.Instance.ClaimActiveNote();
                ToneManager.Instance.PlayNote(claimedActiveNote, LeftMostFrequency * Mathf.Pow(2, 10.0f * currNotch / 1200.0f));

                int currDir = dir;
                do
                {
                    transform.position = new Vector2(transform.position.x + currDir * notchSpacingInWorldCoords, transform.position.y);
                    PlayerManager.Instance.transform.position = new Vector2(PlayerManager.Instance.transform.position.x + currDir * notchSpacingInWorldCoords, PlayerManager.Instance.transform.position.y);
                    currNotch += currDir;
                    yield return new WaitForSeconds(currLagTime);
                    currLagTime = Mathf.Max(currLagTime * HoldLagSpeedUp, MinLagTime);
                    currDir = Math.Sign(PlayerManager.Instance.Input.Player.MoveTone.ReadValue<float>());
                }
                while (dir == currDir && 
                        ((currDir < 0 && currNotch > 0) ||
                        (currDir > 0 && currNotch < notchCount - 1)));
                // while continuing direction, if moving left, isn't at leftmost, or if moving right, isn't at rightmost
            } else
            {
                currLagTime = Mathf.Min(currLagTime / HoldLagSlowDown, InitialHoldLagTime);
            }
            yield return null;
        }
        
    }
}

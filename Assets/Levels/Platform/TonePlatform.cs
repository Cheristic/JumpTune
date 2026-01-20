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

    internal int currNotch;
    internal bool isFixed;
    internal float leftMostFrequency;
    internal float correctFrequency;

    internal float centSpacing;

    [SerializeField] Sprite fixedTileSprite;

    Animator playerAnimator;

    [Header("Outline")]
    [SerializeField] Material outlineMaterial;
    [SerializeField] Material noOutlineMaterial;
    Renderer rend;

    public void Init(bool _Fixed, int _StartingNotch, int _NotchCount, float _NotchSpacingWorld, float _CorrectFrequency, float _CentSpacing, Color _tileDisabledColor) 
    { 
        currNotch = _StartingNotch;
        isFixed = _Fixed;
        notchCount = _NotchCount;
        notchSpacingInWorldCoords = _NotchSpacingWorld;

        centSpacing = _CentSpacing;

        correctFrequency = _CorrectFrequency;
        leftMostFrequency = _CorrectFrequency * Mathf.Pow(2f, -centSpacing * _StartingNotch / 1200f);

        if(isFixed)
        {
            transform.localScale *= new Vector2(2, 2);
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            sr.sprite = fixedTileSprite;
            GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 0.1f); // top ten moments in coding history

            foreach (SpriteRenderer s in this.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                s.color = _tileDisabledColor;
            }
        }

        playerAnimator = FindFirstObjectByType<PlayerControls>().GetComponent<Animator>();
    }

    public double Error()
    {
        if (isFixed) return 0.0f;

        double notchDiff = Math.Round(1200 * Math.Log(CurrFrequency / correctFrequency, 2) / centSpacing);
        return notchDiff * notchDiff;
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
        ToneManager.Instance.PlayNote(CurrFrequency);
        float currLagTime = InitialHoldLagTime;
        while (true)
        {
            int dir = Math.Sign(PlayerManager.Instance.Input.Player.MoveTone.ReadValue<float>());
            if ( dir != 0 && ((dir < 0 && currNotch > 0) || (dir > 0 && currNotch < notchCount - 1)))
            {
                playerAnimator?.SetBool("isSinging", true);
                //int claimedActiveNote = ToneManager.Instance.ClaimActiveNote();
                //ToneManager.Instance.PlayNote(claimedActiveNote, leftMostFrequency * Mathf.Pow(2, centSpacing * currNotch / 1200.0f));

                int currDir = dir;
                do
                {
                    ToneManager.Instance.PlayNote(CurrFrequency);

                    if(!isFixed) {
                        transform.position = new Vector2(transform.position.x + currDir * notchSpacingInWorldCoords, transform.position.y);
                        PlayerManager.Instance.TryMoveByPlatform(currDir * notchSpacingInWorldCoords);    
                        currNotch += currDir;
                    }
                
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
                playerAnimator?.SetBool("isSinging", false);
                currLagTime = Mathf.Min(currLagTime / HoldLagSlowDown, InitialHoldLagTime);
            }
            yield return null;
        }
        
    }

    float CurrFrequency { get => leftMostFrequency * Mathf.Pow(2, centSpacing * currNotch / 1200.0f); }

    private void Start()
    {
        rend = this.transform.GetComponentInChildren<Renderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFixed && PlayerManager.Instance.controls.isGrounded)
        {
            SetOutline();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        SetNoOutline(); 
    }

    private void SetOutline() { rend.material = outlineMaterial; }
    private void SetNoOutline() { rend.material = noOutlineMaterial; }
}

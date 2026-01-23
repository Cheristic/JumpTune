using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class TonePlatform : MonoBehaviour
{
    internal int notchCount;
    internal float notchSpacingInWorldCoords;

    [Header("Links")]
    [SerializeField] Sprite fixedTileSprite;
    [SerializeField] TonePlatformErrorAnim errorAnim;
    [SerializeField] TonePlatformTrigger trigger;
    [SerializeField] ScoreConversions _Conversions;

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

    internal bool hasPlayer = false;


    Animator playerAnimator;

    [Header("Outline")]
    [SerializeField] Material outlineMaterial;
    [SerializeField] Material noOutlineMaterial;
    Renderer rend;

    public void Init(bool _Fixed, int _StartingNotch, int _NotchCount, float _NotchSpacingWorld, float _CorrectFrequency, float _CentSpacing, Color _tileDisabledColor) 
    { 
        isFixed = _Fixed;
        notchCount = _NotchCount;
        notchSpacingInWorldCoords = _NotchSpacingWorld;
        currNotch = _StartingNotch;

        centSpacing = _CentSpacing;

        correctFrequency = _CorrectFrequency;


        if(isFixed)
        {
            leftMostFrequency = _CorrectFrequency * Mathf.Pow(2f, -centSpacing * _StartingNotch / 1200f);

            transform.localScale *= new Vector2(2, 2);
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            sr.sprite = fixedTileSprite;
            var col = GetComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 0.04f); // top ten moments in coding history
            col.offset = new Vector2(0, -.008f);
            trigger.GetComponent<BoxCollider2D>().size = new Vector2(1.5f, 0.1f); // but wait, it gets worse

            foreach (SpriteRenderer s in this.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                s.color = _tileDisabledColor;
            }
        }
        else
        {
            int correctPos = Random.Range(0, notchCount);
            leftMostFrequency = _CorrectFrequency * Mathf.Pow(2f, -centSpacing * correctPos / 1200f);
        }
        playerAnimator = FindFirstObjectByType<PlayerControls>().GetComponent<Animator>();
    }

    public int Error()
    {
        if (isFixed) return 0;

        return Mathf.Abs(Mathf.RoundToInt(1200 * Mathf.Log(CurrFrequency / correctFrequency, 2) / centSpacing));
    }

    public int Score() => _Conversions.ScoreFromError(Error());

    private void OnDisable() => DisableMovement();

    public void EnableMovement()
    {
        hasPlayer = true;
        StartCoroutine(HandleMoveInput());
    }
    public void DisableMovement()
    {
        hasPlayer = false;
        StopAllCoroutines();
    }

    public void PlayPlatformTone()
    {
        //Debug.Log("curr = " + CurrFrequency + ", correct = " + correctFrequency);
        //Debug.Log(Score() + " " + currNotch);
        ToneManager.Instance.PlayNote(CurrFrequency);
    }

    IEnumerator HandleMoveInput()
    { 
        yield return new WaitUntil(() => PlayerManager.Instance.controls.isGrounded);
        PlayPlatformTone();
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

                    if (!isFixed) {
                        transform.position = new Vector2(transform.position.x + currDir * notchSpacingInWorldCoords, transform.position.y);
                        PlayerManager.Instance.TryMoveByPlatform(currDir * notchSpacingInWorldCoords);    
                        currNotch += currDir;
                    }
                    PlayPlatformTone();
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

    public void ShowError()
    {
        StartCoroutine(errorAnim.ErrorAnim(Score()));
    }

}

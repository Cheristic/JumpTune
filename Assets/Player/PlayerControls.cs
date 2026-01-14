using System.Collections;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Net;

public class PlayerControls : MonoBehaviour
{
    [Header("Player Values")]
    [SerializeField] float MOVE_SPEED;
    [SerializeField] float JUMP_HEIGHT;

    [Header("Grounding")]
    [SerializeField] float IS_GROUNDED_CHECK_DISTANCE;
    [SerializeField] LayerMask GroundLayerMask;

    internal PlayerInput input;
    Rigidbody2D rb;
    BoxCollider2D _collider;

    [Header("Jump Physics")]
    [SerializeField] float JUMP_FORCE;
    [SerializeField] float STRONG_DAMP_FORCE;
    [SerializeField] float WEAK_DAMP_FORCE;
    float vy;
    float fy;
    bool jumpHeld;

    // Coyote time
    float coyoteTimer;

    Vector3 leftGroundedChecker = Vector3.zero;
    Vector3 rightGroundedChecker = Vector3.zero;

    // player anim
    Animator animator;

    public void Init()
    {
        input = new();
        rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        input.Enable();
        input.Player.Jump.started += StartJump;
        input.Player.Jump.canceled += ReleasedJump;
        rightGroundedChecker = transform.InverseTransformPoint(new Vector3(_collider.bounds.max.x, _collider.bounds.center.y, 0));
        leftGroundedChecker = transform.InverseTransformPoint(new Vector3(_collider.bounds.min.x, _collider.bounds.center.y, 0));
        animator = GetComponent<Animator>();
    }

    private void OnDisable()
    {
        input?.Disable();
    }
    void FixedUpdate()
    {
        float x_dir = input.Player.Move.ReadValue<Vector2>().x;
        rb.linearVelocityX = x_dir * MOVE_SPEED;

        animator.SetFloat("speed", Mathf.Abs(rb.linearVelocityX));
        if(x_dir != 0) transform.localScale = new Vector3(x_dir, 1, 1);

        if (isGrounded)
        {
            fy = 0;
            rb.gravityScale = 6;
        }
        else if (jumpHeld) fy = JUMP_FORCE;
        else if (rb.linearVelocityY > 0) fy = -STRONG_DAMP_FORCE;
        else if (coyoteTimer <= 0)
        {
            fy = -WEAK_DAMP_FORCE;
            rb.gravityScale = 3;
        }


        // Update coyote timer
        if (isGrounded) coyoteTimer = 0.1f;
        else coyoteTimer = Mathf.Max(coyoteTimer - Time.deltaTime, 0.0f); 

        // Simulate floatiness during player jump for the first timeApplyUpForce or dampen jump if !jumpHeld
        rb.AddForceY(fy);
    }

    float CalculateJumpHeight(float height)
    {
        return Mathf.Sqrt(-2.0f * Physics2D.gravity.y * height);
    }

    void StartJump(InputAction.CallbackContext ctx)
    {
        if (coyoteTimer <= 0) return;

        // Set coyote timer to 0 to avoid double jumps
        coyoteTimer = 0.0f;

        rb.linearVelocityY = CalculateJumpHeight(JUMP_HEIGHT);
        jumpHeld = true;

        animator.SetBool("isJumping", true);
    }

    void ReleasedJump(InputAction.CallbackContext ctx)
    {
        jumpHeld = false;

        animator.SetBool("isJumping", false);
    }

    internal bool isGrounded
    {
        get
        {
            // moving up, definitely not grounded
            if (Math.Abs(rb.linearVelocityY) > .001f) return false;

            RaycastHit2D hit2 = Physics2D.Raycast(transform.TransformPoint(rightGroundedChecker), Vector2.down, IS_GROUNDED_CHECK_DISTANCE, GroundLayerMask);
            if (hit2.collider != null)
            {
                return true;
            }
            RaycastHit2D hit1 = Physics2D.Raycast(transform.TransformPoint(leftGroundedChecker), Vector2.down, IS_GROUNDED_CHECK_DISTANCE, GroundLayerMask);
            if (hit1.collider != null)
            {
                return true;
            }
            return false;
        }
    }
}

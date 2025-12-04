using UnityEngine;
using UnityEngine.InputSystem;

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

    Vector3 leftGroundedChecker = Vector3.zero;
    Vector3 rightGroundedChecker = Vector3.zero;
    public void Init()
    {
        input = new();
        rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        input.Enable();
        input.Player.Jump.started += StartJump;
        rightGroundedChecker = transform.InverseTransformPoint(new Vector3(_collider.bounds.max.x, _collider.bounds.center.y, 0));
        leftGroundedChecker = transform.InverseTransformPoint(new Vector3(_collider.bounds.min.x, _collider.bounds.center.y, 0));
    }

    private void OnDisable()
    {
        input?.Disable();
    }

    void FixedUpdate()
    {
        float x_dir = input.Player.Move.ReadValue<Vector2>().x;
        rb.linearVelocityX = x_dir * MOVE_SPEED;
    }

    void StartJump(InputAction.CallbackContext ctx)
    {
        if (!isGrounded) return;
        rb.linearVelocityY = JUMP_HEIGHT;
    }


    private bool isGrounded
    {
        get
        {

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

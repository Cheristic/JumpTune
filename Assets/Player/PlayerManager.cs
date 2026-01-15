using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public PlayerControls controls;
    public PlayerInput Input => controls.input;
    public Transform ChunkCheckerPoint;
    internal BoxCollider2D _collider;
    [SerializeField] LayerMask WallLayerMask;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        _collider = GetComponent<BoxCollider2D>();
        controls.Init();
    }


    public void TryMoveByPlatform(float amount)
    {
        // if this would push player inside wall, truncate movement amount to wall
        Vector2 boundsCheck = amount > 0 ? new Vector2(_collider.bounds.max.x, _collider.bounds.center.y) :
            new Vector2(_collider.bounds.min.x, _collider.bounds.center.y);
        RaycastHit2D hit = Physics2D.Raycast(boundsCheck, amount > 0 ? Vector2.right : Vector2.left, _collider.size.x/2, WallLayerMask);
        if (hit)
        {
            float curr = amount;
            amount = hit.point.x - boundsCheck.x;
        }
        transform.position = new Vector2(transform.position.x + amount, transform.position.y);
    }
}

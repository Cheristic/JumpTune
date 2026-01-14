using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    public PlayerControls controls;
    public PlayerInput Input => controls.input;
    public Transform ChunkCheckerPoint;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        controls.Init();
    }
}

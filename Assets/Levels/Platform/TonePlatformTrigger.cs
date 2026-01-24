using UnityEngine;

public class TonePlatformTrigger : MonoBehaviour
{
    [SerializeField] TonePlatform TonePlatform;

    private void Start()
    {
        if (LevelManager.Instance.IsPreviewScene) gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TonePlatform.EnableMovement();
        }
    }
    private void OnTriggerExit2D(Collider2D collision) => TonePlatform.DisableMovement();
}

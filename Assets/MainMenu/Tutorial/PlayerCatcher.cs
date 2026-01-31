using UnityEngine;

public class PlayerCatcher : MonoBehaviour
{
    [SerializeField] Vector2 Location;
    [SerializeField] Transform player;

    private void OnEnable()
    {
        player.localPosition = Location;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.localPosition = Location;
        }
    }
}

using UnityEngine;

public class BirdWhistlerSecret : MonoBehaviour
{
    [SerializeField] BoxCollider2D box;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // bad bad bad bad
        {
            Ray ray = new Ray(Input.mousePosition, Vector3.forward);
            if (box.bounds.IntersectRay(ray))
            {
                ToneManager.Instance.sfxPlayer.PlayWhistleSound();
            }
        }
    }
}
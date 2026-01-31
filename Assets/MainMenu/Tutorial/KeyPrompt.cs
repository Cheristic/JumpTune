using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeyPrompt : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Sprite[] frames;
    [SerializeField] float FPS;

    private void OnEnable()
    {
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        int frame = 0;
        while (true)
        {
            image.sprite = frames[frame];
            yield return new WaitForSeconds(1/FPS);
            frame = (frame + 1) % frames.Length;
        }
    }
}

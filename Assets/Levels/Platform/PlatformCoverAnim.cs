using System.Collections;
using UnityEngine;

public class PlatformCoverAnim : MonoBehaviour
{
    [SerializeField] SpriteRenderer _sprite;
    [SerializeField] AnimationCurve FadeCurve;
    [SerializeField] float AnimTime;
    public void Enable()
    {
        gameObject.SetActive(true);
        transform.position = new Vector2(0, transform.position.y);
    }

    public void ChangeOutline(Material mat)
    {
        if (!gameObject.activeInHierarchy) return;

        _sprite.material = mat;
    }

    public void Fade()
    {
        StartCoroutine(FadeAnim());

        IEnumerator FadeAnim()
        {
            float timeProgressed = 0;

            while (timeProgressed < AnimTime)
            {
                float prog = timeProgressed / AnimTime;
                _sprite.color = new Color(_sprite.color.r, _sprite.color.g, _sprite.color.b, FadeCurve.Evaluate(prog));
                timeProgressed = timeProgressed + Time.deltaTime;
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
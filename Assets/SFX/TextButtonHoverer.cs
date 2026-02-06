using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextButtonHoverer : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] float frequency;
    AudioSource source;
    private void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = ToneManager.Instance.audioMixer;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToneManager.Instance.PlayNote(frequency);
    }

}

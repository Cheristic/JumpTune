using UnityEngine;

public class TutorialKeys : MonoBehaviour
{
    double clock = 0.0;
    [SerializeField] double standingOnPlatClockLimit = 2.0;

    private void Update()
    {
        clock += Time.deltaTime;
        if (clock >= standingOnPlatClockLimit)
        {
            DisplayPlatformKeys();
        }
    }

    private void DisplayPlatformKeys()
    {

    }
}

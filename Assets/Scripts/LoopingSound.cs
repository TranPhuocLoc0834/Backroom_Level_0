using UnityEngine;
using System.Collections;

public class LoopingSound : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeInDuration = 2f;
    private bool hasFadedIn = false;

    void Start()
    {
        audioSource.volume = 0f;
        audioSource.loop = true;
        audioSource.Play();
        StartCoroutine(FadeInOnce());
    }

    IEnumerator FadeInOnce()
    {
        if (hasFadedIn) yield break;

        hasFadedIn = true;

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }
        audioSource.volume = 1f; // chắc chắn max
    }
}

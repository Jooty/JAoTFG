using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{

    private Coroutine fadeIn, fadeOut;

    public void StartFadeIn(AudioSource audio, float duration, float targetVolume)
    {
        fadeIn = StartCoroutine(startFadeIn(audio, duration, targetVolume));
    }
    private IEnumerator startFadeIn(AudioSource audio, float duration, float targetVolume)
    {
        audio.Play();
        audio.volume = 0;

        while (audio.volume < targetVolume)
        {
            audio.volume += Time.deltaTime / duration;

            yield return null;
        }
    }

    public void StartFadeOut(AudioSource audio, float duration)
    {
        fadeOut = StartCoroutine(startFadeOut(audio, duration));
    }
    private IEnumerator startFadeOut(AudioSource audio, float duration)
    {
        float startVolume = audio.volume;

        while (audio.volume > 0.01)
        {
            audio.volume -= startVolume * Time.deltaTime / duration;

            yield return null;
        }

        audio.volume = 0;
        audio.Stop();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A temporary object created in the world that plays a sound before deleting itself. UNUSED: See AudioManager's "CreateTempClip()".
/// </summary>
public class TempLocalAudio : MonoBehaviour
{
    [Header("References")]
    public AudioSource source;

    public void Play(AudioClip clip, float volume = -1)
    {
        if (volume != -1)
        {
            source.volume = volume;
        }
        source.clip = clip;

        source.Play();
        StartCoroutine(DestroySelf(clip.length));
    }

    private IEnumerator DestroySelf(float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(this.gameObject);
    }
}

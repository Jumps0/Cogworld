using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Temp Audio Prefab")]
    public GameObject tempAudioPrefab;

    [Header("Audio Sources")]
    public AudioSource globalAmbient;
    public AudioSource globalMusic; // Will probably never be used
    public AudioSource globalOneShot;
    public AudioSource globalCombatSound;
    public AudioSource globalDialogueBackground;

    public AudioSource globalMisc2;
    public AudioSource globalMisc3;
    public AudioSource globalTypingSource;

    [Header("Audio Clips")]
    public List<AudioClip> ambientClips = new List<AudioClip>();
    public List<AudioClip> globalMusicClips = new List<AudioClip>(); // Will probably never be used
    public List<AudioClip> globalMiscClips = new List<AudioClip>();
    //
    public List<AudioClip> DOOR_Clips = new List<AudioClip>();
    public List<AudioClip> TRAPS_Clips = new List<AudioClip>();
    public List<AudioClip> INTRO_Clips = new List<AudioClip>();
    public List<AudioClip> GAME_Clips = new List<AudioClip>();
    public List<AudioClip> UI_Clips = new List<AudioClip>();
    public List<AudioClip> equipItem_Clips = new List<AudioClip>();
    public List<AudioClip> dropItem_Clips = new List<AudioClip>();
    public List<AudioClip> dialogue_Clips = new List<AudioClip>();
    public List<AudioClip> nonBotDestruction_Clips = new List<AudioClip>();
    //
    [Header("  MATERIAL")]
    [Header("      Destroy")]
    [Header("          Robot")]
    public List<AudioClip> RobotDestruction_Clips = new List<AudioClip>();

    public void PlayAmbient(int id, float volume = -1)
    {
        StopAmbient();
        if (volume != -1)
        {
            globalAmbient.volume = volume;
        }
        globalAmbient.loop = true;
        globalAmbient.clip = ambientClips[id];
        globalAmbient.Play();

        
    }

    public void StopAmbient()
    {
        globalAmbient.Stop();
    }

    public void PlayMiscClip(int id, float volume = -1)
    {
        StopMisc();
        if (volume != -1)
        {
            globalOneShot.volume = volume;
        }
        globalOneShot.PlayOneShot(globalMiscClips[id]);
        
    }

    public void StopMisc()
    {
        globalOneShot.Stop();
    }

    public void PlayMusicClip(int id, float volume = -1)
    {
        StopMusic();
        if (volume != -1)
        {
            globalMusic.volume = volume;
        }
        globalMusic.PlayOneShot(globalMusicClips[id]);
        
    }

    public void StopMusic()
    {
        globalMusic.Stop();
    }

    public void PlayMiscSpecific(AudioClip clip, float volume = -1)
    {
        StopMiscSpecific();
        if (volume != -1)
        {
            globalMisc2.volume = volume;
        }
        globalMisc2.PlayOneShot(clip);
        
    }

    public void StopMiscSpecific()
    {
        globalMisc2.Stop();
    }

    public void PlayMiscSpecific2(AudioClip clip, float volume = -1)
    {
        StopMiscSpecific2();
        if (volume != -1)
        {
            globalMisc3.volume = volume;
        }
        globalMisc3.PlayOneShot(clip);

    }

    public void StopMiscSpecific2()
    {
        globalMisc3.Stop();
    }

    public void PlayGlobalCombatSound(AudioClip clip, float volume = -1)
    {
        StopGlobalCombatSound();
        if (volume != -1)
        {
            globalCombatSound.volume = volume;
        }
        globalCombatSound.PlayOneShot(clip);

    }

    public void StopGlobalCombatSound()
    {
        globalCombatSound.Stop();
    }

    public void PlayDialogueAmbient(int id, float volume = -1)
    {
        StopDialogueAmbient();
        if (volume != -1)
        {
            globalDialogueBackground.volume = volume;
        }
        globalDialogueBackground.loop = true;
        globalDialogueBackground.clip = dialogue_Clips[id];
        globalDialogueBackground.Play();
    }

    public void StopDialogueAmbient()
    {
        globalDialogueBackground.Stop();
    }

    public void PlayTyping(float volume = -1)
    {
        StopTyping();
        if (volume != -1)
        {
            globalTypingSource.volume = volume;
        }
        globalTypingSource.loop = true;
        globalTypingSource.clip = UI_Clips[67];
        globalTypingSource.Play();
    }

    public void StopTyping()
    {
        globalTypingSource.Stop();
    }

    public void CreateTempClip(Vector3 location, AudioClip clip, float volume = -1)
    {
        GameObject newAudio = new GameObject();
        newAudio.transform.position = location;
        newAudio.AddComponent<AudioSource>();

        if (volume != -1)
        {
            newAudio.GetComponent<AudioSource>().volume = volume;
        }
        newAudio.GetComponent<AudioSource>().playOnAwake = false;
        newAudio.GetComponent<AudioSource>().loop = false;

        newAudio.GetComponent<AudioSource>().PlayOneShot(clip, volume);

        StartCoroutine(DestroyTempClip(newAudio));
    }

    private IEnumerator DestroyTempClip(GameObject clip)
    {
        while(clip.GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }

        Destroy(clip);
    }
}

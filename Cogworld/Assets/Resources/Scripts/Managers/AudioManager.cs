using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AudioManager : MonoBehaviour
{
    public static AudioManager inst;
    public void Awake()
    {
        inst = this;

        SetupDicts();
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
    // -- Contains the raw clips --
    [Tooltip("Only map ambience in here. Machine ambience gets assigned individually.")]
    [SerializeField] private List<AudioClip> AMBIENT_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> DIALOGUE_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> DOOR_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> ENDINGS_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> EVOLVE_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> GAME_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> GAMEOVER_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> INTRO_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> ITEMS_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> TITLE_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> TRAPS_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> UI_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> ROBOTHACK_Clips = new List<AudioClip>();
    [Header("   MATERIALS")]
    [SerializeField] private List<AudioClip> equipItem_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> dropItem_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> RobotDestruction_Clips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> nonBotDestruction_Clips = new List<AudioClip>();

    [SerializeField] private List<AudioClip> globalMusicClips = new List<AudioClip>(); // Will probably never be used

    // -- DICTIONARIES --
    // Accessed across the project
    public Dictionary<string, AudioClip> dict_ambient;
    public Dictionary<string, AudioClip> dict_dialogue;
    public Dictionary<string, AudioClip> dict_door;
    public Dictionary<string, AudioClip> dict_endings;
    public Dictionary<string, AudioClip> dict_evolve;
    public Dictionary<string, AudioClip> dict_game;
    public Dictionary<string, AudioClip> dict_gameover;
    public Dictionary<string, AudioClip> dict_intro;
    public Dictionary<string, AudioClip> dict_items;
    public Dictionary<string, AudioClip> dict_title;
    public Dictionary<string, AudioClip> dict_traps;
    public Dictionary<string, AudioClip> dict_ui;
    public Dictionary<string, AudioClip> dict_robothack;
    public Dictionary<string, AudioClip> dict_equipitem;
    public Dictionary<string, AudioClip> dict_dropitem;
    public Dictionary<string, AudioClip> dict_robotdestruction;
    public Dictionary<string, AudioClip> dict_nonbotdestruction;

    private void SetupDicts()
    {
        dict_ambient = new Dictionary<string, AudioClip>();

        foreach (var v in AMBIENT_Clips)
        {
            dict_ambient.Add(v.name, v);
        }

        dict_dialogue = new Dictionary<string, AudioClip>();

        foreach (var v in DIALOGUE_Clips)
        {
            dict_dialogue.Add(v.name, v);
        }

        dict_door = new Dictionary<string, AudioClip>();

        foreach (var v in DOOR_Clips)
        {
            dict_door.Add(v.name, v);
        }

        dict_endings = new Dictionary<string, AudioClip>();

        foreach (var v in ENDINGS_Clips)
        {
            dict_endings.Add(v.name, v);
        }

        dict_evolve = new Dictionary<string, AudioClip>();

        foreach (var v in EVOLVE_Clips)
        {
            dict_evolve.Add(v.name, v);
        }

        dict_game = new Dictionary<string, AudioClip>();

        foreach (var v in GAME_Clips)
        {
            dict_game.Add(v.name, v);
        }

        dict_gameover = new Dictionary<string, AudioClip>();

        foreach (var v in GAMEOVER_Clips)
        {
            dict_gameover.Add(v.name, v);
        }

        dict_intro = new Dictionary<string, AudioClip>();

        foreach (var v in INTRO_Clips)
        {
            dict_intro.Add(v.name, v);
        }

        dict_items = new Dictionary<string, AudioClip>();

        foreach (var v in ITEMS_Clips)
        {
            dict_items.Add(v.name, v);
        }

        dict_title = new Dictionary<string, AudioClip>();

        foreach (var v in TITLE_Clips)
        {
            dict_title.Add(v.name, v);
        }

        dict_traps = new Dictionary<string, AudioClip>();

        foreach (var v in TRAPS_Clips)
        {
            dict_traps.Add(v.name, v);
        }

        dict_ui = new Dictionary<string, AudioClip>();

        foreach (var v in UI_Clips)
        {
            dict_ui.Add(v.name, v);
        }

        dict_robothack = new Dictionary<string, AudioClip>();

        foreach (var v in ROBOTHACK_Clips)
        {
            dict_robothack.Add(v.name, v);
        }

        dict_equipitem = new Dictionary<string, AudioClip>();

        foreach (var v in equipItem_Clips)
        {
            dict_equipitem.Add(v.name, v);
        }

        dict_dropitem = new Dictionary<string, AudioClip>();

        foreach (var v in dropItem_Clips)
        {
            dict_dropitem.Add(v.name, v);
        }

        dict_robotdestruction = new Dictionary<string, AudioClip>();

        foreach (var v in RobotDestruction_Clips)
        {
            dict_robotdestruction.Add(v.name, v);
        }

        dict_nonbotdestruction = new Dictionary<string, AudioClip>();

        foreach (var v in nonBotDestruction_Clips)
        {
            dict_nonbotdestruction.Add(v.name, v);
        }
    }

    public void PlayAmbient(int id, float volume = -1)
    {
        StopAmbient();
        if (volume != -1)
        {
            globalAmbient.volume = volume;
        }
        globalAmbient.loop = true;
        globalAmbient.clip = AMBIENT_Clips[id];
        globalAmbient.Play();

        
    }

    public void StopAmbient()
    {
        globalAmbient.Stop();
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
        globalDialogueBackground.clip = DIALOGUE_Clips[id];
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

    public List<AudioClip> activeTempClips = new List<AudioClip>();

    public void CreateTempClip(Vector3 location, AudioClip clip, float volume = -1)
    {
        // Create new temp gameObject
        GameObject newAudio = new GameObject();
        newAudio.transform.SetParent(this.transform);
        newAudio.transform.position = location;
        newAudio.AddComponent<AudioSource>();

        // Add to temp list
        activeTempClips.Add(clip);

        if (volume != -1)
        {
            newAudio.GetComponent<AudioSource>().volume = volume;
        }
        else
        {
            newAudio.GetComponent<AudioSource>().volume = 1f;
        }
        newAudio.GetComponent<AudioSource>().playOnAwake = false;
        newAudio.GetComponent<AudioSource>().loop = false;
        newAudio.GetComponent<AudioSource>().clip = clip;

        //newAudio.GetComponent<AudioSource>().PlayOneShot(clip, volume);
        newAudio.GetComponent<AudioSource>().Play();

        StartCoroutine(DestroyTempClip(newAudio));
    }

    private IEnumerator DestroyTempClip(GameObject clip)
    {
        while(clip.GetComponent<AudioSource>().isPlaying)
        {
            yield return null;
        }

        if(activeTempClips.Contains(clip.GetComponent<AudioSource>().clip))
            activeTempClips.Remove(clip.GetComponent<AudioSource>().clip); // Remove from list

        // Destroy it
        Destroy(clip);
    }

    public GameObject MakeLoopingEffect(AudioClip clip, float volume = -1)
    {
        // Create new temp gameObject
        GameObject newAudio = new GameObject();
        newAudio.transform.SetParent(this.transform);
        newAudio.transform.position = PlayerData.inst.transform.position;
        newAudio.AddComponent<AudioSource>();

        if (volume != -1)
        {
            newAudio.GetComponent<AudioSource>().volume = volume;
        }
        else
        {
            newAudio.GetComponent<AudioSource>().volume = 1f;
        }
        newAudio.GetComponent<AudioSource>().playOnAwake = false;
        newAudio.GetComponent<AudioSource>().loop = true;
        newAudio.GetComponent<AudioSource>().clip = clip;

        //newAudio.GetComponent<AudioSource>().PlayOneShot(clip, volume);
        newAudio.GetComponent<AudioSource>().Play();

        return newAudio;
    }
}

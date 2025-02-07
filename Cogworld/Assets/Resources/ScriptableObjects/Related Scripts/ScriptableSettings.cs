using DungeonResources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SO Systems",menuName = "SO Systems/ScriptableSettings")]
[Tooltip("Contains information regarding player settings, passed from Main Menu to Gameplay Scene and saved inside a ScriptableObject.")]
public class ScriptableSettings : ScriptableObject
{
    [Header("General")]
    public ModalUILayout uiLayout = ModalUILayout.NonModal; // Modal & Non-modal layouts are probably never happening. Here for posterity
    public string font = "TerminusBold"; // Would require a painful refactor of fonts since currently everything relys on it being one font. Here for posterity
    public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;
    public bool showIntro = false;
    public bool tutorial = false;
    public Difficulty difficulty = Difficulty.Explorer;
    public bool logOutput = false;

    [Header("Audio")]
    [Range(0f, 1f)]
    public float volume_master = 1f;
    [Range(0f, 1f)]
    public float volume_interface = 1f;
    [Range(0f, 1f)]
    public float volume_game = 1f;
    [Range(0f, 1f)]
    public float volume_props = 1f;
    [Range(0f, 1f)]
    public float volume_ambient = 1f;
    public bool audioLog = false;

    [Header("Interface")]
    public bool tacticalHud = true;
    [Tooltip("Options are High (False), and Max (True)")]
    public bool combatLogDetail = false;
    public bool partAutoSorting = true;
    public bool inventoryAutoSorting = true;
    [Range(0, 20)] // 0, 5, 10, 15, 20
    public int edgePanningSpeed = 0;
    public bool clickWallsToTarget = false;
    public bool labelSupporterItems = false; // No idea how I would implement this. Here for posterity
    public bool keyBoardMode = false;
    public bool colorblindAdjustment = false;

    [Header("Behavior")]
    public bool autoActivateParts = true;
    public bool stopOnThreatsOnly = true;
    [Range(0, 1500)] // 0, 500, 750, 1000, 1500
    public int moveBlockDuration = 750;

    [Header("Player")]
    [Tooltip("Default is `Player`")]
    public string playerName = "Player";
    public bool uploadScores = false; // Probably will never happen. Here for posterity
    [Tooltip("0 = Random. Default is `Random`")]
    public string seed = "Random";
    public bool newsUpdates = false; // Probably will never happen. Here for posterity
    public bool reportErrors = false; // Probably will never happen. Here for posterity
    public bool achievementsAnywhere = true;

    [Header("Visualization")]
    public bool asciiMode = false;
    public bool showPath = true;
    public bool explosionPredictions = true;
    [Range(0, 3000)] // 0, 1, 500, 1000, 1500, 2000, 3000
    public int hitChanceDelay = 1500;
    public bool combatIndicators = true;
    public bool autoLabelThreats = true;
    public bool autoLabelItems = true;
    public bool autoLabelOnExamine = true;
    public bool colorItemLabels = true;
    [Range(0, 2000)] // 0, 500, 1000, 1500, 2000
    public int motionTrailDuration = 2000;
    [Range(0, 3)] // +0, +1, +2, +3
    public int floorGamma = 0;
    public FOVHandling fovHandling = FOVHandling.FadeIn;
    public bool corruptionGlitches = true;
    public bool screenShake = true;

    [Header("Alerts")]
    [Range(100, 1000)] // 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000
    public int alert_heat = 300;
    [Range(0.1f, 1f)] // 10, 20, 30, 40, 50, 60, 70, 80, 90, 100
    public float alert_core = 0.2f;
    [Range(0.1f, 1f)] // 10, 20, 30, 40, 50, 60, 70, 80, 90, 100
    public float alert_energy = 0.2f;
    [Range(0.1f, 1f)] // 10, 20, 30, 40, 50, 60, 70, 80, 90, 100
    public float alert_matter = 0.1f;
    public bool alertPopups = true;
}

[System.Serializable]
public class ScriptableSettingShort
{
    // Not too pleased with this but unsure of how else to approach it.

    [Header("Basic")]
    public int? value_int = null;
    public float? value_float = null;
    public bool? value_bool = null;
    public string value_string = null;

    [Header("Enums")]
    public ModalUILayout? enum_modal = null;
    public Difficulty? enum_difficulty = null;
    public FOVHandling? enum_fov = null;
    public FullScreenMode? enum_fullscreen = null;

    public bool canBeGrayedOut = false;
    public bool inputfield = false;
    
    // The Great Wall of Optional Variables
    public ScriptableSettingShort(bool grayedOut = false, bool inputfield = false, int? v_i = null, float? v_f = null, bool? v_b = null, string v_s = null, ModalUILayout? e_m = null, Difficulty? e_d = null, FOVHandling? e_fov = null, FullScreenMode? e_fs = null)
    {
        this.value_int = v_i;
        this.value_float = v_f;
        this.value_bool = v_b;
        this.value_string = v_s;

        this.enum_modal = e_m;
        this.enum_difficulty = e_d;
        this.enum_fov = e_fov;
        this.enum_fullscreen = e_fs;

        this.canBeGrayedOut = grayedOut;
    }
}

[System.Serializable]
[Tooltip("Modal & Non-modal layouts are probably never happening. Here for posterity")]
public enum ModalUILayout
{
    NonModal,
    SemiModal,
    Modal
}

[System.Serializable]
public enum Difficulty
{
    Explorer,
    Adventurer,
    Rogue
}

[System.Serializable]
public enum FOVHandling
{
    Delay,
    Instant,
    FadeIn
}

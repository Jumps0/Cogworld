using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class GlobalSettings : MonoBehaviour
{

    public static GlobalSettings inst;
    public void Awake()
    {
        inst = this;
    }

    #region Layer Guide

    /* // THIS IS OLD, GOTO UIMANAGER
     * >===< LAYER GUIDE >===<
     * (Sort order)
     * 1  - Floor/Wall tiles
     * 2  -
     * 3  - 
     * 4  - 
     * 5  - Doors
     * 6  -
     * 7  - Machines
     * 
     * 20 - Player
     * 
     */

    #endregion

    [Header("Cheats")]
    public bool fullVision = false;

    #region Defaults
    [Header("Defaults")]
        [Header("Player")]
    public int viewRange = 15;
    public int startingPowerSlots = 1;
    public int startingPropulsionSlots = 2;
    public int startingUtilitySlots = 2;
    public int startingWeaponSlots = 2;
    //
    public int maxHealth_start = 250;
    public int maxEnergy_start = 100;
    public int maxMatter_start = 300;
    public int startingMatter = 100;
    public int startingEnergy = 100;
    public int startingCorruption = 0;
    public float cogCoreOutput1 = 5.0f;
    public float cogCoreOutput2 = 1.5f;
    public int defaultExposure = 100;
    public float cogCoreHeat1 = -25.0f;
    public float cogCoreHeat2 = -12.5f;
    public int cogCoreDefaultMovement = 50;
    public int cogCoreDefMovement2 = 0;
    public int cogCoreAvoidance = 50;
    public int startingEvasionWide = 0;
    public int startingEvasion = 10;
    //
    public int startingWeight = 3;
    public int startingInvSize = 5;

    #endregion

    [Header("UI")]
    public float itemPopupLifetime = 5;
    public List<TMP_FontAsset> fonts = new List<TMP_FontAsset>();

    #region DebugUI
    [Header("DebugUI")]
    [SerializeField] private GameObject debugUI_parent;
    [SerializeField] private TMP_InputField dField;
    [SerializeField] private Button dButton;
    [SerializeField] private Image dImage1;
    [SerializeField] private Image dImage2;
    [SerializeField] private TextMeshProUGUI dText1;
    [SerializeField] private TextMeshProUGUI dText2;
    private string dString;

    #endregion

    // Update is called once per frame
    void Update()
    {
        CheckForDebug();
        CheckForCheats();
    }
    
    public void SetStartingValues()
    {
        // Set location
        MapManager.inst.currentBranch = 0;
        MapManager.inst.currentLevel = -11;
        MapManager.inst.currentLevelName = "Unknown"; // (Starting cave)
    }

    /// <summary>
    /// Here we generate an "allegance tree" for the default game start for every bot.
    /// </summary>
    /// <param name="info">Accepts an Actor's botInfo to determine its Alignment and Relation.</param>
    /// <returns>Returns a touple list "tree" of relations to every major faction.</returns>
    public Allegance GenerateDefaultAllengances(BotObject info)
    {
        List<(BotAlignment, BotRelation)> tree = new List<(BotAlignment, BotRelation)>();

        if(info == null) // This is probably the player
        {
            tree.Add((BotAlignment.Complex, BotRelation.Hostile));
            tree.Add((BotAlignment.Derelict, BotRelation.Neutral));
            tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
            tree.Add((BotAlignment.Warlord, BotRelation.Neutral));
            tree.Add((BotAlignment.Zion, BotRelation.Neutral));
            tree.Add((BotAlignment.Exiles, BotRelation.Neutral));
            tree.Add((BotAlignment.Architect, BotRelation.Hostile));
            tree.Add((BotAlignment.Subcaves, BotRelation.Neutral));
            tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
            tree.Add((BotAlignment.Player, BotRelation.Friendly));
            tree.Add((BotAlignment.None, BotRelation.Neutral));
            return new Allegance(tree);
        }

        switch (info.locations.alignment)
        {
            case BotAlignment.Complex: // aka 0b10
                tree.Add((BotAlignment.Complex, BotRelation.Friendly));
                tree.Add((BotAlignment.Derelict, BotRelation.Hostile));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Hostile));
                tree.Add((BotAlignment.Zion, BotRelation.Hostile));
                tree.Add((BotAlignment.Exiles, BotRelation.Hostile));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Hostile));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Hostile));
                tree.Add((BotAlignment.None, BotRelation.Hostile));
                return new Allegance(tree);
            case BotAlignment.Derelict:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Friendly));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Friendly));
                tree.Add((BotAlignment.Zion, BotRelation.Friendly));
                tree.Add((BotAlignment.Exiles, BotRelation.Friendly));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Friendly));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Neutral));
                tree.Add((BotAlignment.None, BotRelation.Neutral));
                return new Allegance(tree);
            case BotAlignment.Assembled:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Hostile));
                tree.Add((BotAlignment.Assembled, BotRelation.Friendly));
                tree.Add((BotAlignment.Warlord, BotRelation.Hostile));
                tree.Add((BotAlignment.Zion, BotRelation.Hostile));
                tree.Add((BotAlignment.Exiles, BotRelation.Hostile));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Hostile));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Hostile));
                tree.Add((BotAlignment.None, BotRelation.Hostile));
                return new Allegance(tree);
            case BotAlignment.Warlord:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Friendly));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Friendly));
                tree.Add((BotAlignment.Zion, BotRelation.Friendly));
                tree.Add((BotAlignment.Exiles, BotRelation.Friendly));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Friendly));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Neutral));
                tree.Add((BotAlignment.None, BotRelation.Neutral));
                return new Allegance(tree);
            case BotAlignment.Zion:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Friendly));
                tree.Add((BotAlignment.Assembled, BotRelation.Neutral)); // sus
                tree.Add((BotAlignment.Warlord, BotRelation.Friendly));
                tree.Add((BotAlignment.Zion, BotRelation.Friendly));
                tree.Add((BotAlignment.Exiles, BotRelation.Neutral));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Friendly));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Neutral));
                tree.Add((BotAlignment.None, BotRelation.Neutral));
                return new Allegance(tree);
            case BotAlignment.Exiles:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Friendly));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Friendly));
                tree.Add((BotAlignment.Zion, BotRelation.Neutral));
                tree.Add((BotAlignment.Exiles, BotRelation.Friendly));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Friendly));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Neutral));
                tree.Add((BotAlignment.None, BotRelation.Neutral));
                return new Allegance(tree);
            case BotAlignment.Architect:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Hostile));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Hostile));
                tree.Add((BotAlignment.Zion, BotRelation.Hostile));
                tree.Add((BotAlignment.Exiles, BotRelation.Hostile));
                tree.Add((BotAlignment.Architect, BotRelation.Friendly));
                tree.Add((BotAlignment.Subcaves, BotRelation.Hostile));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Hostile));
                tree.Add((BotAlignment.None, BotRelation.Neutral));
                return new Allegance(tree);
            case BotAlignment.Subcaves:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Neutral));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Neutral));
                tree.Add((BotAlignment.Zion, BotRelation.Neutral));
                tree.Add((BotAlignment.Exiles, BotRelation.Neutral));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Friendly));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Neutral));
                tree.Add((BotAlignment.Player, BotRelation.Neutral));
                tree.Add((BotAlignment.None, BotRelation.Neutral));
                return new Allegance(tree);
            case BotAlignment.SubcavesHostile:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Hostile));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Hostile));
                tree.Add((BotAlignment.Zion, BotRelation.Hostile));
                tree.Add((BotAlignment.Exiles, BotRelation.Hostile));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Neutral));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Hostile));
                tree.Add((BotAlignment.None, BotRelation.Hostile));
                return new Allegance(tree);
            case BotAlignment.Player:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Neutral));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Neutral));
                tree.Add((BotAlignment.Zion, BotRelation.Neutral));
                tree.Add((BotAlignment.Exiles, BotRelation.Neutral));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Neutral));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Friendly));
                tree.Add((BotAlignment.None, BotRelation.Neutral));
                return new Allegance(tree);
            case BotAlignment.None:
                tree.Add((BotAlignment.Complex, BotRelation.Hostile));
                tree.Add((BotAlignment.Derelict, BotRelation.Neutral));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Neutral));
                tree.Add((BotAlignment.Zion, BotRelation.Neutral));
                tree.Add((BotAlignment.Exiles, BotRelation.Neutral));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Neutral));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Neutral));
                tree.Add((BotAlignment.None, BotRelation.Neutral));
                return new Allegance(tree);
            default:
                tree.Add((BotAlignment.Complex, BotRelation.Friendly));
                tree.Add((BotAlignment.Derelict, BotRelation.Hostile));
                tree.Add((BotAlignment.Assembled, BotRelation.Hostile));
                tree.Add((BotAlignment.Warlord, BotRelation.Hostile));
                tree.Add((BotAlignment.Zion, BotRelation.Hostile));
                tree.Add((BotAlignment.Exiles, BotRelation.Hostile));
                tree.Add((BotAlignment.Architect, BotRelation.Hostile));
                tree.Add((BotAlignment.Subcaves, BotRelation.Hostile));
                tree.Add((BotAlignment.SubcavesHostile, BotRelation.Hostile));
                tree.Add((BotAlignment.Player, BotRelation.Hostile));
                tree.Add((BotAlignment.None, BotRelation.Hostile));
                return new Allegance(tree);

        }
    }

    public void SetFont(int id)
    {
        TMP_FontAsset newFont = fonts[id];

        // Gather all text elements
        List<TextMeshProUGUI> textElements = new List<TextMeshProUGUI>(FindObjectsOfType<TextMeshProUGUI>().Select(text => text.GetComponent<TextMeshProUGUI>()));

        foreach (TextMeshProUGUI T in textElements)
        {
            T.font = newFont;
            if(id == 0)
            {
                T.fontSize -= 2;
            }
            else
            {
                T.fontSize += 2;
            }
        }
    }
    
    [Header("DEBUG TESTING")]
    public bool debugitemtest = false;
    public bool debugLeftMessageTest = false;
    public bool debugBottomMessageTest = false;
    public bool debugLogMessageTest = false;
    public ItemObject testitem;
    bool doOnce, doOnce2, doOnce3, doOnce4, doOnce5 = false;

    private void CheckForDebug()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            // Toggle Debug Menu
            debugUI_parent.SetActive(!debugUI_parent.activeInHierarchy);
        }

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            UIManager.inst.NewFloor_BeginAnimate();
        }

        if(debugUI_parent.activeInHierarchy)
        {
            dString = dField.text;
        }

        if (debugitemtest && !doOnce)
        {
            PlayerData.inst.GetComponent<PartInventory>()._inventory.AddItem(new Item(testitem), 1);
            debugitemtest = false;
            doOnce = true;
        }

        if (debugLeftMessageTest && !doOnce2)
        {
            UIManager.inst.CreateLeftMessage("ALERT: Lockdown in effect, collecting threat data.", 10, AudioManager.inst.GAME_Clips[30]);
            doOnce2 = true;
            debugLeftMessageTest = false;
        }

        if (debugBottomMessageTest && !doOnce3)
        {
            UIManager.inst.CreateBottomMessage("This is a test message", "Blue", 10);
            doOnce3 = true;
            debugBottomMessageTest = false;
        }

        if(debugLogMessageTest && !doOnce5)
        {
            UIManager.inst.CreateNewLogMessage("You are now entering the battlezone! Nuking is now legal, worldwide.", UIManager.inst.activeGreen, UIManager.inst.dullGreen);
            doOnce5 = true;
            debugLogMessageTest = false;
        }
    }

    private void CheckForCheats()
    {
        if (fullVision && !doOnce4)
        {
            FogOfWar.inst.DEBUG_RevealAll();
            doOnce4 = false;
        }
    }

    public void DEBUG_CheckDict()
    {
        dImage1.gameObject.SetActive(false);
        dImage2.gameObject.SetActive(false);
        dText1.text = "Layer 0:";
        dText2.text = "Layer 1:";

        if (dString.Contains(",") && !dString.Contains("!")) // 2nd part is a failsafe to stop parsing
        {
            string[] split = dString.Split(",");

            Vector2Int key = new Vector2Int(int.Parse(split[0]), int.Parse(split[1]));

            if (MapManager.inst._allTilesRealized.ContainsKey(key))
            {
                dImage1.gameObject.SetActive(true);
                dText1.text = MapManager.inst._allTilesRealized[key].gameObject.name;
                if (MapManager.inst._allTilesRealized[key].occupied)
                {
                    dText1.text += " (O)";
                }
            }

            if (MapManager.inst._layeredObjsRealized.ContainsKey(key))
            {
                dImage2.gameObject.SetActive(true);
                dText2.text = MapManager.inst._layeredObjsRealized[key].gameObject.name;
            }
        }
    }
}

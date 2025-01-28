using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using UnityEngine.InputSystem;
using System.Security.Cryptography;
using Unity.VisualScripting;

public class GlobalSettings : MonoBehaviour
{

    public static GlobalSettings inst;
    public void Awake()
    {
        inst = this;

#if (UNITY_EDITOR)
        Camera.main.orthographicSize = 20; // So testing in editor isn't nearly impossible to see
#endif
    }

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
    public int innateEnergy = 5;
    public float cogCoreOutput1 = 5.0f;
    public float cogCoreOutput2 = 1.5f;
    public int defaultExposure = 100;
    public float cogCoreHeat1 = -25.0f;
    public float cogCoreHeat2 = -12.5f;
    public float cogCoreHeat3 = 0f;
    public int cogCoreDefaultMovement = 50;
    public int cogCoreDefMovement2 = 0;
    public int cogCoreAvoidance = 50;
    public int startingEvasionWide = 0;
    public int startingEvasion = 10;
    //
    public int startingWeight = 3;
    public int startingInvSize = 5;
    [Tooltip("The minimum % chance the player can be spotted.")] public float minSpotChance = 0.1f;

    public void SetStartingValues()
    {
        // Set location
        MapManager.inst.currentBranch = 0;
        MapManager.inst.currentLevel = -11;
        MapManager.inst.currentLevelName = "Unknown"; // (Starting cave)
        MapManager.inst.levelName = LevelName.Default;
    }
    #endregion

    public float defaultHackingDetectionChance = 0.1f;
    [Tooltip("See inside HF.cs -> TraceHacking() for a better description of what this value is used for.")]
    public float hackingLevelOfFailureBaseBonus = 0.35f;
    [Tooltip("The chance for a prototype to spawn in as 'faulty'.")]
    public float faultyPrototypeChance = 0.10f;
    [Tooltip("It costs a certain amount of matter and energy to be able to attach parts.")]
    public int partMatterAttachmentCost = 10;
    [Tooltip("It costs a certain amount of matter and energy to be able to attach parts.")]
    public int partEnergyAttachmentCost = 20;
    [Tooltip("De-equipping an item expends some amount of energy. (We won't make this a requirement but will remove the amount either way)")]
    public int partEnergyDetachLoss = 10;

    [Header("UI")]
    public float itemPopupLifetime = 5;
    public float globalTextSpeed = 0.35f;
    [Tooltip("0 = COVERAGE | 1 = ENERGY | 2 = INTEGRITY | 3 = INFO")]
    public int defaultItemDataMode = 3;
    [Tooltip("The maximum amount of characters to use in representing a bar. Ex: ||||||||||||")]
    public int maxCharBarLength = 12;
    [Tooltip("The maximum amount of suggestions that can be displayed in the manual hacking suggestion box before the box is considered 'too large' to show.")]
    public int maxHackingSuggestions = 15;
    public List<TMP_FontAsset> fonts = new List<TMP_FontAsset>();
    //
    [Tooltip("When not in view, indicators that hug the border will slowly flash (if true).")]
    public bool animateBorderIndicators = true;

    [Header("Settings")]
    [Tooltip("The scanning/UI animation when the player enters a new level.")]
    public bool showNewLevelAnimation = true;
    [Tooltip("Normal = ###% Advanced = (###) ##")]
    public bool useAdvMoveDisNumbers = true;
    [Tooltip("When a sound and alert should be played if the player exceeds a certain heat level.")]
    public int heatWarningLevel = 300;

    #region DebugUI
    [Header("DebugUI")]
    [SerializeField] private GameObject debugUI_parent; // The dictionary checker
    [SerializeField] private TMP_InputField dField;
    [SerializeField] private Button dButton;
    [SerializeField] private UnityEngine.UI.Image dImage1;
    [SerializeField] private UnityEngine.UI.Image dImage2;
    [SerializeField] private TextMeshProUGUI dText1;
    [SerializeField] private TextMeshProUGUI dText2;
    private string dString = "";

    #endregion

    // Update is called once per frame
    void Update()
    {
        DebugBarLoop();
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

    public void OnToggleDCCheck(InputAction.CallbackContext context)
    {
        // Toggle Debug Menu
        debugUI_parent.SetActive(!debugUI_parent.activeInHierarchy);
    }

    public void OnToggleDebug(InputAction.CallbackContext context)
    {
        ToggleDebugBar();
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (db_main.activeInHierarchy)
        {
            // Read the input
            string command = db_ifield.text;

            // Parse the input and try to do something with that
            if (command.Length >= 2)
            {
                DebugBarDoCommand(command);
            }
        }
    }

    #region Debug Bar
    [Header("Debug Bar")]
    public GameObject db_main;
    [SerializeField] private TMP_InputField db_ifield;
    [SerializeField] private TextMeshProUGUI db_textaid;
    [SerializeField] private TextMeshProUGUI db_playerPosition;
    [SerializeField] private bool db_helper_override = false;
    private Coroutine db_helperCooldown;
    [SerializeField] private List<string> db_commandHistory = new List<string>(); // Tracks past commands which can be re-used

    private void DebugBarLoop()
    {
        // Primary debug bar stuff
        if (db_ifield.gameObject.activeInHierarchy && db_ifield.text.Length > 2) // We want to assist the user and tell them what each thing does
        {
            DebugBarHelper();
        }

        if (db_ifield.gameObject.activeInHierarchy)
        {
            DebugBarHistoryCheck();
        }

        // Pos indicator
        if (db_playerPosition.gameObject.activeInHierarchy && PlayerData.inst)
        {
            Vector2Int playerPos = new Vector2Int((int)PlayerData.inst.transform.position.x, (int)PlayerData.inst.transform.position.y);

            Vector2 m = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2Int mousePos = new Vector2Int((int)(m.x + 0.5f), (int)(m.y + 0.5f)); // Adjustment due to tiles being offset slightly from natural grid

            db_playerPosition.text = $"player  x:{playerPos.x} y:{playerPos.y}" + "\n" + $"mouse   x:{mousePos.x} y:{mousePos.y}";
        }

        // Interfacing mode enforcement
        if(db_ifield.gameObject.activeInHierarchy && db_ifield.isFocused)
        {
            PlayerData.inst.GetComponent<PlayerGridMovement>().UpdateInterfacingMode(InterfacingMode.TYPING);
        }
    }

    private void ToggleDebugBar()
    {
        db_main.SetActive(!db_main.activeInHierarchy);
        db_textaid.gameObject.SetActive(false);

        // Play a sound
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.dict_ui["HACK_BUTTON"], 0.5f); // UI - HACK_BUTTON

        if (db_main.activeInHierarchy)
        {
            DebugBarChangeFocus(true);

            // Switch interfacing mode
            PlayerData.inst.GetComponent<PlayerGridMovement>().UpdateInterfacingMode(InterfacingMode.TYPING);
        }
        else
        {
            // Switch interfacing mode
            PlayerData.inst.GetComponent<PlayerGridMovement>().UpdateInterfacingMode(InterfacingMode.COMBAT);
        }
    }

    private int db_commandHistoryIndex = -1;
    private void DebugBarHistoryCheck()
    {
        if(db_ifield.text.Length == 0){ db_commandHistoryIndex = -1; }
        List<string> history = db_commandHistory;

        // Load previous command from history
        if (db_ifield.isFocused && db_commandHistory.Count > 0)
        {
            // ^ Arrow (Previous Command)
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                // NOTE: Since strings cannot be unique, this has potential to scramble the command history on the chance that two duplicate commands are in the list.
                if (history[0] == db_commandHistory[0]) // Need to make sure the list is reversed
                    history.Reverse();

                if (db_commandHistoryIndex < db_commandHistory.Count - 1)
                {
                    db_commandHistoryIndex++;
                    db_ifield.text = history[db_commandHistoryIndex];
                }
                else if (db_commandHistoryIndex == db_commandHistory.Count - 1)
                {
                    // Clear input when reaching the most recent command
                    //db_commandHistoryIndex = -1;
                    //db_ifield.text = string.Empty;
                    // Or set to last command
                    //db_ifield.text = db_commandHistory[0];
                }
            } // v Down Arrow (Next Command)
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                // NOTE: Since strings cannot be unique, this has potential to scramble the command history on the chance that two duplicate commands are in the list.
                if (history[0] == db_commandHistory[0]) // Need to make sure the list is reversed
                    history.Reverse();

                if (db_commandHistoryIndex > 0)
                {
                    db_commandHistoryIndex--;
                    db_ifield.text = history[db_commandHistoryIndex];
                }
            }

            // Set input field focus and move caret to end
            DebugBarChangeFocus(true);
        }
    }

    private void DebugBarHelper(string override_message = "")
    {
        // Is there a more important message to display?
        if (db_helper_override)
        {
            db_textaid.gameObject.SetActive(true);

            // Yes, change the text and color
            db_textaid.text = override_message;
            db_textaid.color = UIManager.inst.warningOrange;
            return; // Bail out early
        }

        // Normally this should give info about the command the user may be typing, but if an override message is recieved then for X seconds we need to display that message instead.
        if (override_message != "") // Override
        {
            // Start the cooldown, and try again recursively
            if(db_helperCooldown != null)
            {
                StopCoroutine(db_helperCooldown);
            }
            db_helperCooldown = StartCoroutine(DebugBarHelperCooldown());

            DebugBarHelper(override_message);
        }
        else // Normal operations
        {
            db_textaid.gameObject.SetActive(false);
            db_textaid.color = Color.white;

            // Now we need to parse the partial input
            string command = db_ifield.text.ToLower();
            if (command.Length > 2)
            {
                // Check to see if the input contains a command type, if it does, give more detail on that
                if (command.Contains("set"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> set \"target\" \"var\" \"amount\" [Reassign a variable value]";
                }
                else if (command.Contains("spawn"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> spawn \"type\" \"name\" [Spawn something next to the player]";
                }
                else if (command.Contains("fow"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> fow [Toggles the Fog of War]";
                }
                else if (command.Contains("notiles"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> notiles [Toggles vision of all tile sprites]";
                }
                else if (command.Contains("pos"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> pos [Displays current player coordinates]";
                }
                else if (command.Contains("newflooranim"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> newflooranim [Plays the \"new floor\" animation]";
                }
                else if (command.Contains("broadcast"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> broadcast \"type\" \"message\" [Broadcasts a specific type of message]";
                }
                else if (command.Contains("hackfail"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> hackfail [If a terminal is open, forces it to fail.]";
                }
                else if (command.Contains("break"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> break \"item\" [Breaks an equipped item. Use 'random' for a random item.]";
                }
                else if (command.Contains("fuse"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> fuse \"item\" [Fuses an equipped item. Use 'random' for a random item.]";
                }
                else if (command.Contains("faulty"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> faultyitem [Spawns in a random FAULTY prototype beneath the player.]";
                }
                else if (command.Contains("corrupted"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> corrupteditem [Spawns in a random CORRUPTED item beneath the player.]";
                }
                else if (command.Contains("corruptme"))
                {
                    db_textaid.gameObject.SetActive(true);
                    db_textaid.text = "> corruptme \"value\" [Runs a corruption event on the player. (#1-11)]";
                }
                // Expand this as needed
            }
        }
    }

    private IEnumerator DebugBarHelperCooldown()
    {
        db_helper_override = true;

        yield return new WaitForSeconds(5f);

        db_helper_override = false;
        db_textaid.text = "";
    }

    private void DebugBarDoCommand(string istring)
    {
        #region Possible Commands
        /* ===== [ POSSIBLE COMMANDS ] =====
         *  > set "target" "var" "amount"
         *    -Used to set a certain value to an individual target.
         *    Target: The entity we want to target. Can be "player", if not, we need to search for it in the world.
         *    Value: The variable we want to alter (health, energy, matter, corruption, heat)
         *    Amount: The new value the variable should have
         *  > spawn "type" "name"
         *    -Used to spawn something in the world adjacent to the player. Can be an item or an entity
         *    Type: If this is an item or a bot
         *    Name: The direct name of this thing, better spell it correctly!
         *  > fow
         *    -Toggles the fog of war
         *  > notiles
         *    -Toggles vision of all tile sprites
         *  > pos
         *    -Returns (and displays) coordinate position of player, and mouse
         *  > newflooranim
         *    -Plays the "new floor" animation
         *  > broadcast "type" "message"
         *    -Broadcasts a message string with the specified type of broadcaster
         *    Type: The different types of messenger system (alert, info, log)
         *    Message: The message string to display
         *  > hackfail
         *    -If a terminal is open, forces it into the fail state
         *  > break "item"
         *    -Breaks an equipped item. Use 'random' for a random item.
         *  > fuse "item"
         *    -Fuses an equipped item. Use 'random' for a random item.
         *  > faultyitem
         *    -Spawns in a random FAULTY prototype beneath the player.
         *  > corrupteditem
         *    -Spawns in a random CORRUPTED item beneath the player.
         *  > corruptme "value"
         *    -Runs a corruption event on the player (#1-11).
         * ================================
         */
        #endregion

        bool success = false;

        // Split the command into parts
        istring = istring.ToLower();
        string[] bits = istring.Split(" ");

        switch (bits[0])
        {
            case "set":
                // Important variables:
                Actor c_target = null;
                string value = "";
                float amount = 0f;

                // Next go to the second word
                if(bits.Length == 1) // There is no second word
                {
                    DebugBarHelper("[set] Must specify target (ex. player)");
                    return;
                }
                else
                {
                    // Try to parse the target
                    string target = bits[1];
                    if (target.Contains("player")) // Player is target
                    {
                        c_target = PlayerData.inst.GetComponent<Actor>();
                    }
                    else // This isn't the player, and we will need to search for this bot in the world
                    {
                        foreach (Entity E in GameManager.inst.entities)
                        {
                            if(E.uniqueName == target)
                            {
                                c_target = E.GetComponent<Actor>();
                            }
                        }
                    }

                    // Do we have a valid target?
                    if(c_target == null)
                    {
                        DebugBarHelper("[set] Target not found");
                        return;
                    }
                    else // Continue
                    {
                        // Do we have a 3rd word?
                        if(bits.Length <= 2)
                        {
                            DebugBarHelper("[set] Must specify a variable to change (ex. energy)");
                            return;
                        }
                        else
                        {
                            // Is the 3rd word valid?
                            value = bits[2];

                            if(value == "health" || value == "energy" || value == "matter" || value == "corruption" || value == "heat")
                            {
                                // Yes, continue to the 4th and final word.
                                if(bits.Length <= 3)
                                {
                                    DebugBarHelper("[set] Please specify an amount to change the value by");
                                    return;
                                }
                                else
                                {
#pragma warning disable CS0168
                                    // Don't need to parse this, but we will clamp if
                                    try
                                    {
                                        amount = Mathf.Clamp(float.Parse(bits[3]), -5000f, 5000f);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        amount = 0;
                                        DebugBarHelper("[set] Must specify a valid number");
                                        return;
                                    }
                                }
                            }
                            else // Bail out here
                            {
                                DebugBarHelper("[set] Unknown variable");
                                return;
                            }
                        }
                    }
                }

                // Now that we have collected all our info, we can actually do the command
                switch (value)
                {
                    case "health":
                        if (c_target.GetComponent<PlayerData>())
                        {
                            PlayerData.inst.currentHealth = (int)amount;
                            DebugBarHelper($"Player {value} set to {(int)amount}.");
                        }
                        else
                        {
                            c_target.currentHealth += (int)amount;
                            DebugBarHelper($"{c_target.uniqueName}'s {value} set to {(int)amount}.");
                        }
                        break;
                    case "energy":
                        if (c_target.GetComponent<PlayerData>())
                        {
                            PlayerData.inst.currentEnergy = (int)amount;
                            DebugBarHelper($"Player {value} set to {(int)amount}.");
                        }
                        else
                        {
                            c_target.currentEnergy += (int)amount;
                            DebugBarHelper($"{c_target.uniqueName}'s {value} set to {(int)amount}.");
                        }
                        break;
                    case "matter":
                        if (c_target.GetComponent<PlayerData>())
                        {
                            PlayerData.inst.currentMatter = (int)amount;
                            DebugBarHelper($"Player {value} set to {(int)amount}.");
                        }
                        else
                        {
                            // Bots don't have matter
                        }
                        break;
                    case "corruption":
                        if (c_target.GetComponent<PlayerData>())
                        {
                            PlayerData.inst.currentCorruption = (int)amount;
                            DebugBarHelper($"Player {value} set to {(int)amount}.");
                        }
                        else
                        {
                            c_target.corruption += (int)amount;
                            DebugBarHelper($"{c_target.uniqueName}'s {value} set to {(int)amount}.");
                        }
                        break;
                    case "heat":
                        if (c_target.GetComponent<PlayerData>())
                        {
                            PlayerData.inst.currentHeat = (int)amount;
                            DebugBarHelper($"Player {value} set to {(int)amount}.");
                        }
                        else
                        {
                            c_target.currentHeat += (int)amount;
                            DebugBarHelper($"{c_target.uniqueName}'s {value} set to {(int)amount}.");
                        }
                        break;
                    default:
                        // Do nothing? This shouldn't happen?
                        break;
                }
                success = true;

                break;
            case "spawn":
                // Important variables:
                string tospawn = "";
                ItemObject tospawn_item = null;
                BotObject tospawn_bot = null;

                // Second word
                if (bits.Length == 1) // There is no second word
                {
                    DebugBarHelper("[spawn] Must specify thing type (ex. item OR bot)");
                    return;
                }
                else
                {
                    // Try to parse the target
                    string thing = bits[1];
                    if (thing.Contains("bot"))
                    {
                        tospawn = "bot";
                    }
                    else if (thing.Contains("item"))
                    {
                        tospawn = "item";
                    }

                    // Valid option chosen?
                    if(tospawn == "")
                    {
                        DebugBarHelper("[spawn] Must specify valid type (ex. item)");
                        return;
                    }
                    else // Continue
                    {
                        // Lastly, utilize the rest of the input (which will be a full name with spaces included)
                        string obj = istring.Split(bits[1])[1].Trim();
                        // Need to now search for this thing based on type we got earlier (we will just go through the databases)
                        if(thing == "bot")
                        {
                            foreach (var B in MapManager.inst.botDatabase.Bots)
                            {
                                if(B.botName.ToLower() == obj || B.botName.ToLower().Contains(obj)) // Maybe make this more forgiving
                                {
                                    tospawn_bot = B;
                                }
                            }
                        }
                        else if(thing == "item")
                        {
                            foreach (var I in MapManager.inst.itemDatabase.Items)
                            {
                                if (I.itemName.ToLower() == obj || I.itemName.ToLower().Contains(obj)) // Maybe make this more forgiving
                                {
                                    tospawn_item = I;
                                }
                            }
                        }
                    }

                }

                // Now do the command since we have what we need

                // We want to spawn it as close to the player as possible
                Vector2Int playerloc = new Vector2Int((int)PlayerData.inst.transform.position.x, (int)PlayerData.inst.transform.position.y);

                switch (tospawn)
                {
                    case "bot":
                        MapManager.inst.PlaceBot(playerloc, tospawn_bot);
                        DebugBarHelper($"Spawned a {tospawn_bot.botName}.");

                        success = true;

                        // Update FOV/FOW
                        GameManager.inst.AllActorsVisUpdate();
                        break;

                    case "item":
                        InventoryControl.inst.CreateItemInWorld(new ItemSpawnInfo(tospawn_item.itemName, playerloc, 1, true));
                        DebugBarHelper($"Spawned a {tospawn_item.itemName}.");

                        success = true;

                        // Update FOV/FOW
                        GameManager.inst.AllActorsVisUpdate();
                        break;
                }

                break;
            case "fow":
                FogOfWar.inst.DEBUG_ToggleFog();
                success = true;

                break;
            case "notiles":
                foreach (var T in MapManager.inst._allTilesRealized)
                {
                    T.Value.bottom._renderer.enabled = !T.Value.bottom._renderer.enabled;
                }

                success = true;

                break;
            case "pos":
                Vector2Int playerPos = new Vector2Int((int)PlayerData.inst.transform.position.x, (int)PlayerData.inst.transform.position.y);
                Debug.Log($"Player position: {playerPos}");
                DebugBarHelper($"Player position: {playerPos}");

                // Toggle the text
                db_playerPosition.gameObject.SetActive(!db_playerPosition.gameObject.activeInHierarchy);

                success = true;

                break;
            case "newflooranim":
                UIManager.inst.NewFloor_BeginAnimate();
                DebugBarHelper("Playing animation...");

                success = true;

                break;
            case "broadcast":
                string broadcast_message = "";
                int broadcast_type = -1;

                // Next go to the second word
                if (bits.Length == 1) // There is no second word
                {
                    DebugBarHelper("[broadcast] Must specify type (ex. alert, info, log)");
                    return;
                }
                else
                {
                    // Try to parse the type
                    string type = bits[1];
                    if (type.Contains("alert"))
                    {
                        broadcast_type = 0;
                    }
                    else if (type.Contains("info"))
                    {
                        broadcast_type = 1;
                    }
                    else if (type.Contains("log"))
                    {
                        broadcast_type = 2;
                    }

                    // Valid type?
                    if (broadcast_type == -1)
                    {
                        DebugBarHelper("[broadcast] Invalid broadcast type");
                        return;
                    }

                    // Do we have a 3rd word?
                    if (bits.Length <= 2)
                    {
                        DebugBarHelper("[broadcast] Must specify a message to display (ex. Hello world)");
                        return;
                    }
                    else
                    {
                        // Get the whole message
                        broadcast_message = istring.Split(bits[1])[1].Trim();
                    }
                }

                // Now do the message
                switch (broadcast_type)
                {
                    case 0:
                        UIManager.inst.CreateLeftMessage(broadcast_message, 10, AudioManager.inst.dict_game["FACILITY_ALERT"]); // GAME - FACILITY_ALERT
                        DebugBarHelper("Broadcasting alert message...");
                        break;
                    case 1:
                        UIManager.inst.CreateBottomMessage(broadcast_message, new List<Color>() {GameManager.inst.colors.bm_blue_dark, GameManager.inst.colors.bm_blue_norm, GameManager.inst.colors.bm_blue_text }, 10);
                        DebugBarHelper("Broadcasting info message...");
                        break;
                    case 2:
                        UIManager.inst.CreateNewLogMessage(broadcast_message, UIManager.inst.activeGreen, UIManager.inst.dullGreen);
                        DebugBarHelper("Broadcasting log message...");
                        break;
                    // Add more cases as needed
                }

                success = true;

                break;
            case "hackfail":

                if(UIManager.inst.terminal_targetTerm != null)
                {
                    // Need to get current hack status values
                    bool detected = UIManager.inst.terminal_targetTerm.detected;

                    if (!detected)
                    {
                        UIManager.inst.Terminal_InitTrace(); // Activate the trace bar
                    }

                    foreach (var item in UIManager.inst.terminal_hackinfoList.ToList())
                    {
                        if (item.GetComponent<UITraceBar>())
                        {
                            item.GetComponent<UITraceBar>().ExpandByPercent(1f);

                            DebugBarHelper("Failing hack...");

                            return;
                        }
                    }
                }
                else
                {
                    DebugBarHelper("[FAILED] No hack in progress.");
                }

                // For this special case, we will mark it as successful
                success = true;

                break;
            case "break":
                // Second word
                if (bits.Length == 1) // There is no second word
                {
                    DebugBarHelper("[break] Must specify item name or 'random'");
                    return;
                }
                else
                {
                    // Try to parse
                    string target = bits[1];
                    if (target.Contains("random"))
                    {
                        // Destroy random part
                        InvDisplayItem idi = HF.GetRandomPlayerPart();

                        if(idi != null && idi.item != null)
                        {
                            HF.BreakPart(idi.item, idi);

                            DebugBarHelper($"Broke {idi.nameUnmodified}.");
                            success = true;
                        }
                        else
                        {
                            DebugBarHelper($"Failed to find an item to break.");
                        }
                    }
                    else
                    {
                        // Try to find this item on the UI
                        List<Item> items = Action.CollectAllBotItems(PlayerData.inst.GetComponent<Actor>());
                        InvDisplayItem idi = HF.GetInvDisplayItemByName(target);

                        if (idi != null)
                        {
                            HF.BreakPart(idi.item, idi);

                            DebugBarHelper($"Broke {idi.nameUnmodified}.");
                            success = true;
                        }
                        else
                        {
                            DebugBarHelper($"Failed to find item '{target}'.");
                        }
                    }
                }
                break;
            case "fuse":
                // Second word
                if (bits.Length == 1) // There is no second word
                {
                    DebugBarHelper("[fuse] Must specify item name or 'random'");
                    return;
                }
                else
                {
                    // Try to parse
                    string target = bits[1];
                    if (target.Contains("random"))
                    {
                        // Get random part
                        InvDisplayItem idi = HF.GetRandomPlayerPart();

                        if (idi != null && idi.item != null)
                        {
                            HF.FusePart(idi.item, idi);

                            DebugBarHelper($"Fused {idi.nameUnmodified}.");
                            success = true;
                        }
                        else
                        {
                            DebugBarHelper($"Failed to find an item to fuse.");
                        }
                    }
                    else
                    {
                        // Try to find this item on the UI
                        List<Item> items = Action.CollectAllBotItems(PlayerData.inst.GetComponent<Actor>());
                        InvDisplayItem idi = HF.GetInvDisplayItemByName(target);

                        if (idi != null)
                        {
                            HF.FusePart(idi.item, idi);

                            DebugBarHelper($"Fused {idi.nameUnmodified}.");
                            success = true;
                        }
                        else
                        {
                            DebugBarHelper($"Failed to find item '{target}'.");
                        }
                    }
                }
                break;
            case "faultyitem":
                // Get a prototype item
                ItemObject toSpawn = HF.FindItemOfTier(Random.Range(4, 9), true);

                // Spawn it in as faulty
                if(toSpawn != null)
                {
                    InventoryControl.inst.CreateItemInWorld(new ItemSpawnInfo(toSpawn.itemName, HF.LocationOfPlayer(), 1, false, 1.0f));

                    DebugBarHelper($"Spawned a faulty {toSpawn.itemName}.");

                    success = true;
                }
                else
                {
                    DebugBarHelper("[faultyitem] failed to find an item to spawn. Please try again.");
                    return;
                }

                break;
            case "corrupteditem":
                // Get an item
                ItemObject corrItem = HF.FindItemOfTier(3, false);

                // Spawn it in as corrupted
                if (corrItem != null)
                {
                    InventoryControl.inst.CreateItemInWorld(new ItemSpawnInfo(corrItem.itemName, HF.LocationOfPlayer(), 1, false, 0, 100));

                    DebugBarHelper($"Spawned a corrupted {corrItem.itemName}.");

                    success = true;
                }
                else
                {
                    DebugBarHelper("[corrupteditem] failed to find an item to spawn. Please try again.");
                    return;
                }

                break;
            case "corruptme":
                int eventID = 0;

                // Second word
                if (bits.Length == 1) // There is no second word
                {
                    DebugBarHelper("[corruptme] Must specify an event value (Any number from 1 to 11)");
                    return;
                }
                else
                {
                    // Safety parse
                    bool good_parse = int.TryParse(bits[1], out int ignored);

                    if (good_parse)
                    {
                        eventID = int.Parse(bits[1]);
                    }
                    else
                    {
                        DebugBarHelper($"[corruptme] Failed to parse \"{eventID}\". Please try again.");
                    }

                    // Valid option chosen?
                    if (eventID < 1 || eventID > 11)
                    {
                        DebugBarHelper("[corruptme] Must be a value from 1 to 11.");
                        return;
                    }
                    else // Do the thing
                    {
                        Action.CorruptionConsequences(eventID);

                        DebugBarHelper($"Ran corruption event with ID = {eventID}.");

                        success = true;
                        break;
                    }

                }
            default: // Unknown command
                DebugBarHelper("Unknown command...");
                return;
        }

        if (success) // If we have successfully performed a command, we have a few things to finish up with
        {
            // Switch interfacing mode
            PlayerData.inst.GetComponent<PlayerGridMovement>().UpdateInterfacingMode(InterfacingMode.COMBAT);

            // Save command
            db_commandHistory.Add(istring);

            // Clear the input box
            db_ifield.text = "";

            // Play a sound so the player knows its a success
            AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_ui["CASH_REGISTER"], 0.35f);

            // Unfocus
            DebugBarChangeFocus(false);
        }
    }

    private void DebugBarChangeFocus(bool focus)
    {
        if (focus)
        {
            db_ifield.Select();
            db_ifield.ActivateInputField();
            db_ifield.caretPosition = db_ifield.text.Length;
        }
        else
        {
            db_ifield.DeactivateInputField();
            db_ifield.caretPosition = 0;
        }
    }

    #endregion

    public void DEBUG_CheckDict()
    {
        dImage1.gameObject.SetActive(false);
        dImage2.gameObject.SetActive(false);
        dText1.text = "Layer 0:";
        dText2.text = "Layer 1:";

        dString = dField.text;

        if (dString.Contains(",") && !dString.Contains("!")) // 2nd part is a failsafe to stop parsing
        {
            string[] split = dString.Split(",");

            Vector2Int key = new Vector2Int(int.Parse(split[0]), int.Parse(split[1]));

            if (MapManager.inst._allTilesRealized.ContainsKey(key)) // Bottom layer
            {
                dImage1.gameObject.SetActive(true);
                dText1.text = MapManager.inst._allTilesRealized[key].bottom.gameObject.name;
                if (MapManager.inst._allTilesRealized[key].bottom.occupied)
                {
                    dText1.text += " (O)";
                }
            }
            // Top layer
            if (MapManager.inst._allTilesRealized.ContainsKey(key) && MapManager.inst._allTilesRealized[key].top != null)
            {
                dImage2.gameObject.SetActive(true);
                dText2.text = MapManager.inst._allTilesRealized[key].top.gameObject.name;
            }
        }
    }

    #region Input Actions
    public PlayerInputActions inputActions;
    private void OnEnable()
    {
        inputActions = Resources.Load<InputActionsSO>("Inputs/InputActionsSO").InputActions;

        inputActions.Player.ToggleDebug.performed += OnToggleDebug;
        inputActions.Player.ToggleDCCheck.performed += OnToggleDCCheck;
        inputActions.Player.Enter.performed += OnSubmit;
    }

    private void OnDisable()
    {
        inputActions.Player.ToggleDebug.performed -= OnToggleDebug;
        inputActions.Player.ToggleDCCheck.performed -= OnToggleDCCheck;
        inputActions.Player.Enter.performed -= OnSubmit;
    }
    #endregion

    /*
    // Size of each grid cell
    public float cellSize = 15f;

    // Number of rows and columns in the grid
    public int numRows = 10;
    public int numColumns = 10;

    private void OnDrawGizmos()
    {
        // Set the color of the grid lines
        Gizmos.color = Color.white;

        // Calculate the total width and height of the grid
        float totalWidth = cellSize * numColumns;
        float totalHeight = cellSize * numRows;

        // Calculate the starting position for drawing the grid
        Vector3 startPosition = transform.position - new Vector3(totalWidth / 2f, totalHeight / 2f, 0f);

        // Draw vertical lines
        for (int x = 0; x <= numColumns; x++)
        {
            Vector3 start = startPosition + new Vector3(x * cellSize, 0f, 0f);
            Vector3 end = start + new Vector3(0f, totalHeight, 0f);
            Gizmos.DrawLine(start, end);
        }

        // Draw horizontal lines
        for (int y = 0; y <= numRows; y++)
        {
            Vector3 start = startPosition + new Vector3(0f, y * cellSize, 0f);
            Vector3 end = start + new Vector3(totalWidth, 0f, 0f);
            Gizmos.DrawLine(start, end);
        }
    }
    */
}

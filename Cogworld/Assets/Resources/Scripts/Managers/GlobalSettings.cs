using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using UnityEngine.InputSystem;

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

    [Header("Cheats")]
    public bool cheat_fullVision = false;

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

    [Header("UI")]
    public float itemPopupLifetime = 5;
    public float globalTextSpeed = 0.35f;
    [Tooltip("0 = COVERAGE | 1 = ENERGY | 2 = INTEGRITY | 3 = INFO")]
    public int defaultItemDataMode = 3;
    [Tooltip("The maximum amount of characters to use in representing a bar. Ex: ||||||||||||")]
    public int maxCharBarLength = 12;
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
    //bool doOnce, doOnce2, doOnce3, doOnce4, doOnce5 = false;

    private void CheckForDebug()
    {
        /*
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
            UIManager.inst.CreateLeftMessage("ALERT: Lockdown in effect, collecting threat data.", 10, AudioManager.inst.GAME_Clips[32]); // FACILITY_ALERT
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
        */
    }

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
            string command = db_input.text;

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
    [SerializeField] private TMP_InputField db_input;
    [SerializeField] private TextMeshProUGUI db_textaid;
    [SerializeField] private TextMeshProUGUI db_playerPosition;
    [SerializeField] private bool db_helper_override = false;
    private Coroutine db_helperCooldown;
    private List<string> db_commandHistory = new List<string>(); // Tracks past commands which can be re-used

    private void DebugBarLoop()
    {
        // Primary debug bar stuff
        if (db_input.gameObject.activeInHierarchy && db_input.text.Length > 2) // We want to assist the user and tell them what each thing does
        {
            DebugBarHelper();
        }

        if (db_input.gameObject.activeInHierarchy)
        {
            DebugBarHistoryCheck();
        }

        // Pos indicator
        if (db_playerPosition.gameObject.activeInHierarchy && PlayerData.inst)
        {
            Vector2Int playerPos = new Vector2Int((int)PlayerData.inst.transform.position.x, (int)PlayerData.inst.transform.position.y);
            db_playerPosition.text = $"x:{playerPos.x} y:{playerPos.y}";
        }

        // Interfacing mode enforcement
        if(db_input.gameObject.activeInHierarchy && db_input.isFocused)
        {
            PlayerData.inst.GetComponent<PlayerGridMovement>().UpdateInterfacingMode(InterfacingMode.TYPING);
        }
    }

    private void ToggleDebugBar()
    {
        db_main.SetActive(!db_main.activeInHierarchy);
        db_textaid.gameObject.SetActive(false);

        // Play a sound
        AudioManager.inst.CreateTempClip(this.transform.position, AudioManager.inst.UI_Clips[42], 0.5f); // UI - HACK_BUTTON

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
        // Load previous command from history
        if (db_input.isFocused && db_commandHistory.Count > 0)
        {
            // ^ Arrow (Previous Command)
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                if (db_commandHistoryIndex < db_commandHistory.Count - 1)
                {
                    db_commandHistoryIndex++;
                    db_input.text = db_commandHistory[db_commandHistoryIndex];
                }
                else if (db_commandHistoryIndex == db_commandHistory.Count - 1)
                {
                    // Clear input when reaching the most recent command
                    //db_commandHistoryIndex = -1;
                    //db_input.text = string.Empty;
                    // Or set to last command
                    //db_input.text = db_commandHistory[0];
                }
            }

            // v Down Arrow (Next Command)
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                if (db_commandHistoryIndex > 0)
                {
                    db_commandHistoryIndex--;
                    db_input.text = db_commandHistory[db_commandHistoryIndex];
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
            string command = db_input.text.ToLower();
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

    private void DebugBarDoCommand(string input)
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
         *  > mypos
         *    -Returns coordinate position of player
         * ================================
         */

        bool success = false;

        // Split the command into parts
        input = input.ToLower();
        string[] bits = input.Split(" ");

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

                // Second wrod
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
                        string obj = input.Split(bits[1])[1].Trim();
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
                        InventoryControl.inst.CreateItemInWorld(tospawn_item.data.Id, playerloc, true);
                        DebugBarHelper($"Spawned a {tospawn_item.itemName}.");

                        success = true;

                        // Update FOV/FOW
                        GameManager.inst.AllActorsVisUpdate();
                        break;
                }

                break;
            case "fow":
                FogOfWar.inst.DEBUG_RevealAll();
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

            default: // Unknown command
                DebugBarHelper("Unknown command...");
                return;
        }

        if (success) // If we have successfully performed a command, we have a few things to finish up with
        {
            // Switch interfacing mode
            PlayerData.inst.GetComponent<PlayerGridMovement>().UpdateInterfacingMode(InterfacingMode.COMBAT);

            // Save command
            db_commandHistory.Add(input);

            // Clear the input box
            db_input.text = "";

            // Unfocus
            DebugBarChangeFocus(false);
        }

        #endregion
    }

    private void DebugBarChangeFocus(bool focus)
    {
        if (focus)
        {
            db_input.Select();
            db_input.ActivateInputField();
            db_input.caretPosition = db_input.text.Length;
        }
        else
        {
            db_input.DeactivateInputField();
            db_input.caretPosition = 0;
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
        inputActions.Player.Pickup.performed += OnSubmit;
    }

    private void OnDisable()
    {
        inputActions.Player.ToggleDebug.performed -= OnToggleDebug;
        inputActions.Player.ToggleDCCheck.performed -= OnToggleDCCheck;
        inputActions.Player.Pickup.performed -= OnSubmit;
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

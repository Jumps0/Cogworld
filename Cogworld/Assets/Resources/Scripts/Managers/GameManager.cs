using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;
    public InputActionsSO inputSO;


    [Header("Data Related")]
    private iDataService DataService = new JsonDataService();
    public SaveData data;

    private void Awake()
    {
        if(inst == null)
        {
            inst = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // -- Maybe move the stuff below somewhere else? (later) --

        data = new SaveData(); // Set Default values for save

        CanDeserializeSavaDataJson(); // Try to load save data from .json file

        if (!CanDeserializeSavaDataJson()) // If that fails
        {
            CreateNewSavaDataJson(); // Make a new one
        }

        gameDifficulty = data.mode;
        hardDiff = data.difficulty;

        GameManager.inst.GrantSchematicKnowledge(MapManager.inst.itemDatabase.Items[2]);
        GameManager.inst.GrantSchematicKnowledge(MapManager.inst.itemDatabase.Items[3]);
        GameManager.inst.GrantSchematicKnowledge(MapManager.inst.itemDatabase.Items[4]);
        GameManager.inst.GrantSchematicKnowledge(MapManager.inst.itemDatabase.Items[5]);
        //GameManager.inst.GrantSchematicKnowledge(null, MapManager.inst.botDatabase.Bots[2]);

        questEvents = new QuestEvents(); // Setup the event listener
    }

    #region File I/O (.json)

    // Methods
    // Writes and saves player's save data to the JSON file; Returns if successful or not
    public bool CanSerializeSavaDataJson()
    {
        if (DataService.SaveData("/save-data.json", data))
        {
            //Debug.Log("Save Data can be Saved...");

            return true;
        }
        else
        {
            Debug.LogError("Could not save the save data.");

            return false;
        }
    }

    // Writes and saves the save data to the JSON file
    public void SerializeSavaDataJson()
    {
        if (DataService.SaveData("/save-data.json", data))
        {
            //Debug.Log("Save Data Saved");
        }
        else
        {
            Debug.LogError("Could not save the save data.");
        }
    }

    /*
     Reads and saves the save data from the JSON file.
    If the file does not exist, the save will not be initialized.
        In this case, the save will copy the save data provided in the MainMenuMgr,
        where the data is initialized in the Unity Editor Inspection Window.
    Returns if succesful or not
     */
    public bool CanDeserializeSavaDataJson()
    {
        try
        {
            data = DataService.LoadData<SaveData>("/save-data.json");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not read file." + " - Thrown Exception: " + e);

            return false;
        }
    }

    /*
    Reads and saves the save data from the JSON file.
    If the file does not exist, the save will not be initialized.
    In this case, the save will copy the save data provided in the MainMenuMgr,
    where the data is initialized in the Unity Editor Inspection Window.
 */
    public void DeserializeSavaDataJson()
    {
        try
        {
            data = DataService.LoadData<SaveData>("/save-data.json");
        }
        catch (Exception e)
        {
            Debug.LogError($"Could not read file." + " - Thrown Exception: " + e);
        }
    }

    public void CreateNewSavaDataJson()
    {
        data = new SaveData();
        CanSerializeSavaDataJson();
    }

    // Save the player's current status from PlayerData to the .json file. Called externally.
    public void SavePlayerStatus(int layer, string layerName, int newSeed, int branchValue, int storedMatter, int turn, Vector2Int core1, Vector2Int core2, int killCount)
    {
        data = new SaveData(layer, layerName, newSeed, branchValue, storedMatter, gameDifficulty, hardDiff, turn, core1, core2, killCount);

        if (CanSerializeSavaDataJson())
        {
            SerializeSavaDataJson();
        }

    }

    #endregion

    public List<Entity> entities = new List<Entity>();
    public List<Entity> Entities { get => entities; }

    [Header("AI Groups")]
    public List<GroupLeader> activeAssaults = new List<GroupLeader>();
    public List<GroupLeader> activeInvestigations = new List<GroupLeader>();
    public List<GroupLeader> activeExterminations = new List<GroupLeader>();
    public List<GroupLeader> activeReinforcements = new List<GroupLeader>();
    public List<GroupLeader> activePatrols = new List<GroupLeader>();
    // Expand this later...

    [Header("Game Options")]
    [Tooltip("If true, the mouse will be the primary way the player moves around. (As opposed to using the keyboard)")]
    public bool allowMouseMovement = true;
    [Tooltip("0 = Novice, 1 = Explorer, 2 = Rogue")]
    public int mode = 0;
    [Tooltip("False = Normal, True = Hard")]
    public bool hardDiff = false;

    [Header("Variables")]
    [SerializeField] private int enemyCount;
    [SerializeField] private int botCount;
    [SerializeField] private float turnTimeDelay = 0.1f;

    [Header("Alert")]
    /*
     * Alert goes from:
     * Low Security (#) aka 0 - (0 to 100)
     *                Level 1 - (200 to 400)
     *                Level 2 - (400 to 600)
     *                Level 3 - (600 to 800)
     *                Level 4 - (800 to 1000)
     *                Level 5 - (1000 to 1200)
     *  HIGH SECURTIY   aka 6 - (1200+)
     */
    [Tooltip("Goes from 0 to 6, see GameManager.cs for details")]
    public int alertLevel = 0;
    [Tooltip("Goes from 0 to 1200+, see GameManager.cs for details")]
    public int alertValue = 0;
    [Tooltip("Major map condition if you go all out violence mode. Gradual increasing temperature until everything melts.")]
    public bool alert_steralization = false; // TODO: Steralization

    [Header("Event Listening")]
    public QuestEvents questEvents;

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
    }

    public void InsertEntity(Entity entity, int index)
    {
        entities.Insert(index, entity);
    }

    public Actor GetBlockingActorAtLocation(Vector3 location)
    {
        foreach (Actor actor in TurnManager.inst.actors)
        {
            if (actor.BlocksMovement && actor.transform.position == location)
            {
                return actor;
            }
        }
        return null;
    }

    /// <summary>
    /// Updates the field of view for every existing entity. Updates player FOV as well
    /// </summary>
    public void AllActorsVisUpdate()
    {
        foreach (Entity E in entities)
        {
            E.GetComponent<Actor>().UpdateFieldOfView();
        }
    }

    /// <summary>
    /// Called whenever the player moves. Updates vision on objects in current and adjacent regions.
    /// </summary>
    public void UpdateNearbyVis()
    {
        Vector2Int pos = HF.V3_to_V2I(PlayerData.inst.transform.position); // Get player's position

        // Get the position of the current region
        Vector2Int currentRegionPosition = pos / MapManager.inst.regionSize;
        // Iterate through the current region and adjacent regions
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                Vector2Int adjacentRegionPosition = currentRegionPosition + new Vector2Int(xOffset, yOffset);
                Debug.Log($"Attempting to update: {adjacentRegionPosition}");
                if (MapManager.inst.regions.ContainsKey(adjacentRegionPosition))
                {
                    // Call UpdateVis for the region if it exists
                    Debug.Log($"Updating vis for {adjacentRegionPosition}");
                    MapManager.inst.regions[adjacentRegionPosition].UpdateVis();
                }
            }
        }

        Debug.Log($"Region count is: {MapManager.inst.regions.Count}");
        foreach (var R in MapManager.inst.regions)
        {
            Debug.Log($"Region: {R.Key} | {R.Value}");
        }
    }

    /// <summary>
    /// Given a location, will attempt to update any neighboring doors to open/close.
    /// </summary>
    /// <param name="pos">The central position. Any neighbors will get updated.</param>
    public void LocalDoorUpdate(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        neighbors.Add(pos); // Center

        // Neighbors
        neighbors.Add(pos + Vector2Int.up);
        neighbors.Add(pos + Vector2Int.up + Vector2Int.left);
        neighbors.Add(pos + Vector2Int.up + Vector2Int.right);
        neighbors.Add(pos + Vector2Int.down);
        neighbors.Add(pos + Vector2Int.down + Vector2Int.left);
        neighbors.Add(pos + Vector2Int.down + Vector2Int.right);
        neighbors.Add(pos + Vector2Int.left);
        neighbors.Add(pos + Vector2Int.right);

        // Neighbors + 1 (for closing)
        neighbors.Add(pos + Vector2Int.up * 2);
        neighbors.Add(pos + Vector2Int.up * 2 + Vector2Int.right);
        neighbors.Add(pos + Vector2Int.up * 2 + Vector2Int.left);
        neighbors.Add(pos + Vector2Int.up * 2 + Vector2Int.right * 2);
        neighbors.Add(pos + Vector2Int.up * 2 + Vector2Int.left * 2);
        neighbors.Add(pos + Vector2Int.down * 2);
        neighbors.Add(pos + Vector2Int.down * 2 + Vector2Int.right);
        neighbors.Add(pos + Vector2Int.down * 2 + Vector2Int.left);
        neighbors.Add(pos + Vector2Int.down * 2 + Vector2Int.right * 2);
        neighbors.Add(pos + Vector2Int.down * 2 + Vector2Int.left * 2);
        neighbors.Add(pos + Vector2Int.left * 2);
        neighbors.Add(pos + Vector2Int.left * 2 + Vector2Int.up);
        neighbors.Add(pos + Vector2Int.left * 2 + Vector2Int.down);
        neighbors.Add(pos + Vector2Int.right * 2);
        neighbors.Add(pos + Vector2Int.right * 2 + Vector2Int.up);
        neighbors.Add(pos + Vector2Int.right * 2 + Vector2Int.down);

        foreach (var L in neighbors)
        {
            if (MapManager.inst._allTilesRealized.ContainsKey(L))
            {
                GameObject go = MapManager.inst._allTilesRealized[L].top;

                if(go && go.GetComponent<DoorLogic>() != null)
                {
                    go.GetComponent<DoorLogic>().StateCheck();
                }
            }
        }

    }

    private void Start()
    {
        // - Startup Logic -
        //HandleStartup(); // No more! We auto-load now. See `Start()` in MapManager.

        // - Button Listener Adding -
        confirm_button.onClick.AddListener(ConfirmExitEvolution);
        //
        powerButtonL.onClick.AddListener(EP_Decrease_Power);
        powerButtonR.onClick.AddListener(EP_Increase_Power);
        propulsionButtonL.onClick.AddListener(EP_Decrease_Propulsion);
        propulsionButtonR.onClick.AddListener(EP_Increase_Propulsion);
        utilityButtonL.onClick.AddListener(EP_Decrease_Utility);
        utilityButtonR.onClick.AddListener(EP_Increase_Utility);
        weaponButtonL.onClick.AddListener(EP_Decrease_Weapon);
        weaponButtonR.onClick.AddListener(EP_Increase_Weapon);
    }

    private void Update()
    {
        if (doEvolutionCheck)
        {
            EvolutionCheck();
        }
    }

    #region Start-Up

    [Header("Start-up UI")]
    [Tooltip("This is where the LNG/LSG/LIH info gets assigned.")]
    [SerializeField] private GameObject startupCanvas;
    // Launch New Game
    [Header("Load New Game (LNG)")]
    [SerializeField] private GameObject lngPanel;
    [SerializeField] private TextMeshProUGUI lngMode_text;
    [SerializeField] private TextMeshProUGUI lngModeInteract_text;
    [SerializeField] private TextMeshProUGUI lngDifficulty_text;
    [SerializeField] private TextMeshProUGUI lngDifficultyInteract_text;
    // Launch Saved Game
    [Header("Load Saved Game (LSG)")]
    [SerializeField] private GameObject lsgPanel;
    [SerializeField] private TextMeshProUGUI lsgMode_text;
    [SerializeField] private TextMeshProUGUI lsgModeInteract_text;
    [SerializeField] private TextMeshProUGUI lsgDifficulty_text;
    [SerializeField] private TextMeshProUGUI lsgDifficultyInteract_text;
    [SerializeField] private TextMeshProUGUI lsgDepth_text;
    [SerializeField] private TextMeshProUGUI lsgDepthInteract_text;
    [SerializeField] private TextMeshProUGUI lsgTurn_text;
    [SerializeField] private TextMeshProUGUI lsgTurnInteract_text;
    [SerializeField] private TextMeshProUGUI lsgCoreSetup_text;
    [SerializeField] private TextMeshProUGUI lsgCoreSetupInteract_text;
    [SerializeField] private TextMeshProUGUI lsgRobokills_text;
    [SerializeField] private TextMeshProUGUI lsgRobokillsInteract_text;
    // Load into Hideout
    [Header("Load into Hideout (LIH)")]
    [SerializeField] private GameObject lihPanel;
    [SerializeField] private TextMeshProUGUI lihDepth_text;
    [SerializeField] private TextMeshProUGUI lihLayerName_text;

    [Header("Game Values")]
    public int gameDifficulty = 0; // 0 = Novice, 1 = Explorer, 2 = Rogue
    public int hardMode = 0; // False = Normal, True = Hard

    public void HandleStartup()
    {
        startupCanvas.SetActive(true); // Turn on the canvas

        lngPanel.SetActive(false);
        lsgPanel.SetActive(false);
        lihPanel.SetActive(false);
    }

    public void ExitStartup()
    {
        startupCanvas.SetActive(false); // Turn off the canvas


    }

    public void LaunchNewGame() // ======
    {
        ExitStartup();

        StartCoroutine(MapManager.inst.InitNewLevel());
    }

    public void LNG_ShowGameSettings()
    {
        lngPanel.SetActive(true);
        // Show current game settings (difficulty, type, etc...)
        switch (mode)
        {
            case 0: // Novice
                lngModeInteract_text.text = "Novice";
                break;
            case 1: // Explorer
                lngModeInteract_text.text = "Explorer";
                break;
            case 2: // Rogue
                lngModeInteract_text.text = "Rogue";
                break;

            default:
                lngModeInteract_text.text = "ERROR: Mode not valid!";
                Debug.LogError("Mode not valid!");
                break;
        }

        if (hardDiff)
        {
            lngDifficultyInteract_text.text = "Hard";
        }
        else
        {
            lngDifficultyInteract_text.text = "Normal";
        }
    }

    public void LNG_HideGameSettings()
    {
        lngPanel.SetActive(false);

    }

    public void LaunchSavedGame() // ======
    {
        ExitStartup();


    }

    public void LSG_ShowSaveDetails()
    {
        lsgPanel.SetActive(true);
        // -- Retrieve info from save file --
        switch (data.mode)
        {
            case 0: // Novice
                lsgModeInteract_text.text = "Novice";
                break;
            case 1: // Explorer
                lsgModeInteract_text.text = "Explorer";
                break;
            case 2: // Rogue
                lsgModeInteract_text.text = "Rogue";
                break;

            default:
                lsgModeInteract_text.text = "ERROR: Mode not valid!";
                Debug.LogError("Mode not valid!");
                break;
        }

        if (data.difficulty)
        {
            lsgDifficultyInteract_text.text = "Hard";
        }
        else
        {
            lsgDifficultyInteract_text.text = "Normal";
        }

        lsgDepthInteract_text.text = data.layer + "/" + data.layerName;

        lsgTurnInteract_text.text = data.turn.ToString();

        lsgCoreSetupInteract_text.text = data.core1.x.ToString() + "/" + data.core1.y.ToString() + "/" + data.core2.x.ToString() + "/" + data.core2.y.ToString();

        lsgRobokillsInteract_text.text = data.killCount.ToString();

    }

    public void LSG_HideSaveDetails()
    {
        lsgPanel.SetActive(false);

    }

    public void LoadIntoHideout() // ======
    {
        ExitStartup();

        BaseManager.inst.TryLoadIntoBase();
    }

    public void LIH_ShowHideoutInfo()
    {
        lihPanel.SetActive(true);
        // Show hideout details
        // (BaseManager should have already loaded this!)
        lihDepth_text.text = "Depth: " + BaseManager.inst.data.layer.ToString();
        lihLayerName_text.text = "Layer: " + BaseManager.inst.data.layerName;

    }

    public void LIH_HideHideoutInfo()
    {
        lihPanel.SetActive(false);

    }

    #endregion

    #region Patrol Management

    public List<GroupLeader> groups = new List<GroupLeader>();

    public void CreatePatrolRoutes()
    {

    }

    public void PatrolCleanUp()
    {

    }

    #endregion

    #region Evolution
    [Header("Evolution Page")]
    private bool doEvolutionCheck = false;
    public GameObject evolution_ref;
    public TextMeshProUGUI _binary;
    public TextMeshProUGUI _matrix;
    public Color greenDark;
    public Color grayedOut;
    public Color verygrayedOut;
    public Color yellowFilled;
    public Color yellowGrayedOut;
    public Color greenIn;
    public int upgradesToApply = 0;
    [Header("--Apply")]
    public Image applyBacking;
    public TextMeshProUGUI _apply;
    public GameObject params_ref;
    public GameObject confirm_ref;
    public Button confirm_button;
    [Header("--PPUW")]
    public TextMeshProUGUI _powerNum;
    public TextMeshProUGUI _powerNum2;
    public TextMeshProUGUI _propulsionNum;
    public TextMeshProUGUI _propulsionNum2;
    public TextMeshProUGUI _utilityNum;
    public TextMeshProUGUI _utilityNum2;
    public TextMeshProUGUI _weaponNum;
    public TextMeshProUGUI _weaponNum2;
    public Image powerBacking;
    public Image propulsionBacking;
    public Image utilityBacking;
    public Image weaponBacking;
    public Button powerButtonL;
    public Button powerButtonR;
    public Button propulsionButtonL;
    public Button propulsionButtonR;
    public Button utilityButtonL;
    public Button utilityButtonR;
    public Button weaponButtonL;
    public Button weaponButtonR;

    [SerializeField] private int stringLength = 100; // Binary
    [SerializeField] private int string2Length = 100; // Matrix

    public void OpenEvolutionScreen(int depth)
    {
        // Early game is less
        switch (depth)
        {
            case -11:
                upgradesToApply += 1;
                break;
            case -10:
                upgradesToApply += 1;
                break;
            case -9:
                upgradesToApply += 1;
                break;
            default:
                upgradesToApply += 2; // Default is 2
                break;
        }

        evolution_ref.SetActive(true);
        StartBinary();
        

        SetDefaultNums();
        EvolveButtonVisuals();

        doEvolutionCheck = true;
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_evolve["SCAN_7"]); // EVOLVE SCAN_7
    }

    public void CloseEvolutionScreen()
    {
        StopBinary();

        evolution_ref.SetActive(false);

        doEvolutionCheck = false;

        // Update player's save
        if (CanDeserializeSavaDataJson())
        {
            SavePlayerStatus(data.layer, data.layerName, data.mapSeed, data.branchValue, data.storedMatter, data.turn, new Vector2Int(PlayerData.inst.powerSlots, PlayerData.inst.propulsionSlots),
                new Vector2Int(PlayerData.inst.utilitySlots, PlayerData.inst.weaponSlots), data.killCount);
        }

        StartCoroutine(MapManager.inst.InitNewLevel()); // All done, generate the new level
    }

    int usedPoints = 0;

    public void EvolutionCheck()
    {
        // Yellow backing stuff
        if (pPower_d < pPower_new)
        {
            powerBacking.color = yellowFilled;
        }
        else
        {
            powerBacking.color = greenIn;
        }
        if (pProp_d < pProp_new)
        {
            propulsionBacking.color = yellowFilled;
        }
        else
        {
            propulsionBacking.color = greenIn;
        }
        if (pUtil_d < pUtil_new)
        {
            utilityBacking.color = yellowFilled;
        }
        else
        {
            utilityBacking.color = greenIn;
        }
        if (pWep_d < pWep_new)
        {
            weaponBacking.color = yellowFilled;
        }
        else
        {
            weaponBacking.color = greenIn;
        }

        UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new);

        // Set text
        _apply.text = "APPLY " + (upgradesToApply - usedPoints).ToString();

        if (usedPoints == upgradesToApply)
        {
            // Gray out apply
            applyBacking.color = greenDark;

            // Show confirm/exit thing
            EnableConfirmExit();
        }
        else
        {
            // Green in apply
            applyBacking.color = greenIn;

            DisableConfirmExit();
        }
    }

    public void EvolveButtonVisuals()
    {
        if(pPower_new > pPower_d && usedPoints == upgradesToApply) // We can decrease but can't increase
        {
            // Highlight decrease button & gray out increase
            powerButtonL.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
            powerButtonR.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
        }
        else if (pPower_new > pPower_d && usedPoints < upgradesToApply) // We can decrease & increase
        {
            // Highlight both decrease & increase buttons
            powerButtonL.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
            powerButtonR.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
        }
        else if(pPower_new <= pPower_d && usedPoints < upgradesToApply) // We can increase but can't decrease
        {
            // Highlight increase button & gray out decrease
            powerButtonL.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
            powerButtonR.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
        }
        else // Can't do anything
        {
            // Gray out both
            powerButtonL.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
            powerButtonR.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
        }
        // ------------------------------------------------------------------------------------------------------------------------- //
        if (pProp_new > pProp_d && usedPoints == upgradesToApply) // We can decrease but can't increase
        {
            // Highlight decrease button & gray out increase
            propulsionButtonL.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
            propulsionButtonR.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
        }
        else if (pProp_new > pProp_d && usedPoints < upgradesToApply) // We can decrease & increase
        {
            // Highlight both decrease & increase buttons
            propulsionButtonL.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
            propulsionButtonR.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
        }
        else if (pProp_new <= pProp_d && usedPoints < upgradesToApply) // We can increase but can't decrease
        {
            // Highlight increase button & gray out decrease
            propulsionButtonL.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
            propulsionButtonR.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
        }
        else // Can't do anything
        {
            // Gray out both
            propulsionButtonL.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
            propulsionButtonR.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
        }
        // ------------------------------------------------------------------------------------------------------------------------- //
        if (pUtil_new > pUtil_d && usedPoints == upgradesToApply) // We can decrease but can't increase
        {
            // Highlight decrease button & gray out increase
            utilityButtonL.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
            utilityButtonR.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
        }
        else if (pUtil_new > pUtil_d && usedPoints < upgradesToApply) // We can decrease & increase
        {
            // Highlight both decrease & increase buttons
            utilityButtonL.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
            utilityButtonR.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
        }
        else if (pUtil_new <= pUtil_d && usedPoints < upgradesToApply) // We can increase but can't decrease
        {
            // Highlight increase button & gray out decrease
            utilityButtonL.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
            utilityButtonR.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
        }
        else // Can't do anything
        {
            // Gray out both
            utilityButtonL.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
            utilityButtonR.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
        }
        // ------------------------------------------------------------------------------------------------------------------------- //
        if (pWep_new > pWep_d && usedPoints == upgradesToApply) // We can decrease but can't increase
        {
            // Highlight decrease button & gray out increase
            weaponButtonL.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
            weaponButtonR.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
        }
        else if (pWep_new > pWep_d && usedPoints < upgradesToApply) // We can decrease & increase
        {
            // Highlight both decrease & increase buttons
            weaponButtonL.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
            weaponButtonR.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
        }
        else if (pWep_new <= pWep_d && usedPoints < upgradesToApply) // We can increase but can't decrease
        {
            // Highlight increase button & gray out decrease
            weaponButtonL.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
            weaponButtonR.GetComponentInChildren<TextMeshProUGUI>().color = greenIn;
        }
        else // Can't do anything
        {
            // Gray out both
            weaponButtonL.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
            weaponButtonR.GetComponentInChildren<TextMeshProUGUI>().color = grayedOut;
        }
    }

    public void EnableConfirmExit()
    {
        confirm_ref.SetActive(true);
        params_ref.SetActive(false);
    }

    public void DisableConfirmExit()
    {
        confirm_ref.SetActive(false);
        params_ref.SetActive(true);
    }

    // This is the one that actually changes things!
    public void SetNewEvolveValues()
    {

        MapManager.inst.tempPlayer.GetComponent<PlayerData>().powerSlots = pPower_new;
        MapManager.inst.tempPlayer.GetComponent<PlayerData>().propulsionSlots = pProp_new;
        MapManager.inst.tempPlayer.GetComponent<PlayerData>().utilitySlots = pUtil_new;
        MapManager.inst.tempPlayer.GetComponent<PlayerData>().weaponSlots = pWep_new;

        /*
        InventoryControl.inst.ClearInterfacesInventories();

        // This might hurt
        if (pPower_new - pPower_d != 0)
        {
            InventoryControl.inst.p_inventoryPower.Container.Items = ResizeISArray(InventoryControl.inst.p_inventoryPower.Container.Items, pPower_new);
        }

        if (pProp_new - pProp_d != 0)
        {
            InventoryControl.inst.p_inventoryPropulsion.Container.Items = ResizeISArray(InventoryControl.inst.p_inventoryPropulsion.Container.Items, pProp_new);
        }

        if (pUtil_new - pUtil_d != 0)
        {
            InventoryControl.inst.p_inventoryUtilities.Container.Items = ResizeISArray(InventoryControl.inst.p_inventoryUtilities.Container.Items, pUtil_new);
        }

        if (pWep_new - pWep_d != 0)
        {
            InventoryControl.inst.p_inventoryWeapons.Container.Items = ResizeISArray(InventoryControl.inst.p_inventoryWeapons.Container.Items, pWep_new);
        }
        */

        MapManager.inst.logEvoChanges = true;
        MapManager.inst.evoChanges = new List<int>();
        MapManager.inst.evoChanges.Add(pPower_new - pPower_d);
        MapManager.inst.evoChanges.Add(pProp_new - pProp_d);
        MapManager.inst.evoChanges.Add(pUtil_new - pUtil_d);
        MapManager.inst.evoChanges.Add(pWep_new - pWep_d);

        // Also apply evo boosts to player
        PlayerData.inst.maxHealth += GlobalSettings.inst.preferences.evolve_newHealthPerLevel;
        PlayerData.inst.naturalHeatDissipation += 3;
        // And refresh their core values
        PlayerData.inst.NewLevelRestore();
    }

    public InventorySlot[] ResizeISArray(InventorySlot[] array, int length)
    {
        InventorySlot[] newArray = new InventorySlot[length];

        if (length > array.Length)
        {
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }
        }
        else if (length < array.Length)
        {
            for (int i = 0; i < length; i++)
            {
                newArray[i] = array[i];
            }
        }

        return newArray;
    }

    // - Button Events -

    public void ConfirmExitEvolution()
    {
        SetNewEvolveValues();
        CloseEvolutionScreen();
    }

    private void EP_Increase_Power()
    {
        if(usedPoints < upgradesToApply) // Have points to use
        {
            pPower_new += 1; // Update new
            usedPoints += 1;
            UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new); // Update text
            EvolveButtonVisuals(); // Update button visuals
        }
    }

    private void EP_Decrease_Power()
    {
        if (pPower_new > pPower_d) // Have points to use
        {
            pPower_new -= 1; // Update new
            usedPoints -= 1;
            UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new); // Update text
            EvolveButtonVisuals(); // Update button visuals
        }
    }

    private void EP_Increase_Propulsion()
    {
        if (usedPoints < upgradesToApply) // Have points to use
        {
            pProp_new += 1; // Update new
            usedPoints += 1;
            UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new); // Update text
            EvolveButtonVisuals(); // Update button visuals
        }
    }

    private void EP_Decrease_Propulsion()
    {
        if (pProp_new > pProp_d) // Have points to use
        {
            pProp_new -= 1; // Update new
            usedPoints -= 1;
            UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new); // Update text
            EvolveButtonVisuals(); // Update button visuals
        }
    }

    private void EP_Increase_Utility()
    {
        if (usedPoints < upgradesToApply) // Have points to use
        {
            pUtil_new += 1; // Update new
            usedPoints += 1;
            UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new); // Update text
            EvolveButtonVisuals(); // Update button visuals
        }
    }

    private void EP_Decrease_Utility()
    {
        if (pUtil_new > pUtil_d) // Have points to use
        {
            pUtil_new -= 1; // Update new
            usedPoints -= 1;
            UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new); // Update text
            EvolveButtonVisuals(); // Update button visuals
        }
    }

    private void EP_Increase_Weapon()
    {
        if (usedPoints < upgradesToApply) // Have points to use
        {
            pWep_new += 1; // Update new
            usedPoints += 1;
            UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new); // Update text
            EvolveButtonVisuals(); // Update button visuals
        }
    }

    private void EP_Decrease_Weapon()
    {
        if (pWep_new > pWep_d) // Have points to use
        {
            pWep_new -= 1; // Update new
            usedPoints -= 1;
            UpdateEvolveNumbers(pPower_new, pProp_new, pUtil_new, pWep_new); // Update text
            EvolveButtonVisuals(); // Update button visuals
        }
    }

    // -              -

    int pPower_d = 0;
    int pProp_d = 0;
    int pUtil_d = 0;
    int pWep_d = 0;
    int pPower_new = 0;
    int pProp_new = 0;
    int pUtil_new = 0;
    int pWep_new = 0;

    public void SetDefaultNums()
    {
        // Set default number values (getting from tempPlayer may be risky!)
        int pPower = MapManager.inst.tempPlayer.GetComponent<PlayerData>().powerSlots;
        int pProp = MapManager.inst.tempPlayer.GetComponent<PlayerData>().propulsionSlots;
        int pUtil = MapManager.inst.tempPlayer.GetComponent<PlayerData>().utilitySlots;
        int pWep = MapManager.inst.tempPlayer.GetComponent<PlayerData>().weaponSlots;
        pPower_d = (int)pPower;
        pProp_d = (int)pProp;
        pUtil_d = (int)pUtil;
        pWep_d = (int)pWep;
        pPower_new = (int)pPower;
        pProp_new = (int)pProp;
        pUtil_new = (int)pUtil;
        pWep_new = (int)pWep;


        UpdateEvolveNumbers(pPower, pPower, pUtil, pWep);
    }

    public void UpdateEvolveNumbers(int pPower, int pProp, int pUtil, int pWep)
    {
        //Debug.Log("Default: " + pPower_d + " ," + pProp_d + " ," + pUtil_d + " ," + pWep_d);
        //Debug.Log("New: " + pPower_new + " ," + pProp_new + " ," + pUtil_new + " ," + pWep_new);

        if (pPower > 9) // 2 digits
        {
            _powerNum.color = Color.black;
            _powerNum.text = (pPower / 10).ToString();
            _powerNum2.color = Color.black;
            _powerNum2.text = pPower.ToString();
        }
        else
        {
            if(powerBacking.color == greenIn) // Green
            {
                _powerNum.color = verygrayedOut;
            }
            else // Yellow
            {
                _powerNum.color = yellowGrayedOut;
            }
            _powerNum.text = "0";
            _powerNum2.color = Color.black;
            _powerNum2.text = pPower.ToString();
        }

        if (pProp > 9) // 2 digits
        {
            _propulsionNum.color = Color.black;
            _propulsionNum.text = (pProp / 10).ToString();
            _propulsionNum2.color = Color.black;
            _propulsionNum2.text = pProp.ToString();
        }
        else
        {
            if (propulsionBacking.color == greenIn) // Green
            {
                _propulsionNum.color = verygrayedOut;
            }
            else // Yellow
            {
                _propulsionNum.color = yellowGrayedOut;
            }
            _propulsionNum.text = "0";
            _propulsionNum2.color = Color.black;
            _propulsionNum2.text = pProp.ToString();
        }

        if (pUtil > 9) // 2 digits
        {
            _utilityNum.color = Color.black;
            _utilityNum.text = (pUtil / 10).ToString();
            _utilityNum2.color = Color.black;
            _utilityNum2.text = pUtil.ToString();
        }
        else
        {
            if (utilityBacking.color == greenIn) // Green
            {
                _utilityNum.color = verygrayedOut;
            }
            else // Yellow
            {
                _utilityNum.color = yellowGrayedOut;
            }
            _utilityNum.text = "0";
            _utilityNum2.color = Color.black;
            _utilityNum2.text = pUtil.ToString();
        }

        if (pWep > 9) // 2 digits
        {
            _weaponNum.color = Color.black;
            _weaponNum.text = (pWep / 10).ToString();
            _weaponNum2.color = Color.black;
            _weaponNum2.text = pWep.ToString();
        }
        else
        {
            if (weaponBacking.color == greenIn) // Green
            {
                _weaponNum.color = verygrayedOut;
            }
            else // Yellow
            {
                _weaponNum.color = yellowGrayedOut;
            }
            _weaponNum.text = "0";
            _weaponNum2.color = Color.black;
            _weaponNum2.text = pWep.ToString();
        }
    }

    public void StartBinary()
    {
        InvokeRepeating("UpdateText", 0f, 1f);
        InvokeRepeating("UpdateMatrix", 0f, 1f);
    }

    public void StopBinary()
    {
        CancelInvoke("UpdateText");
        CancelInvoke("UpdateMatrix");
    }

    private void UpdateText()
    {
        var randomBinaryChars = new char[stringLength];
        for (int i = 0; i < stringLength; i++)
        {
            randomBinaryChars[i] = (Random.Range(0, 2) == 0) ? '0' : '1';
        }
        _binary.text = new string(randomBinaryChars);
    }

    private void UpdateMatrix()
    {
        string randomAlphabet = "";
        for (int i = 0; i < string2Length; i++)
        {
            char randomLetter = (char)('A' + Random.Range(0, 26));
            randomAlphabet += randomLetter;
        }
        _matrix.text = randomAlphabet;
    }

    public void EvoButtonSound()
    {
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["MODE_ON"], 0.5f); // UI - MODE_ON
    }

    public void EvoHoverSound()
    {
        AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["HOVER"], 0.5f); // UI - HOVER
    }

    #endregion

    #region Global Actions Realized

    public void AccessMain()
    {

    }

    public void AccessBranch()
    {

    }

    public void AccessEmergency(GameObject target)
    {
        if (target.GetComponent<Terminal>())
        {
            target.GetComponent<Terminal>().zone.RevealLocalEAccess();
        }
        else
        {
            Debug.LogError("ERROR: Tried to reveal emergency access of non-terminal machine.");
        }
    }

    #region Machine Revealing
    public void IndexMachinesGeneric(int id)
    {
        // We don't need to do any messaging for this since the hack reward already handled that

        StartCoroutine(IndexMachinesAction(id));
    }

    private IEnumerator IndexMachinesAction(int id)
    {
        // We need to stall while the terminal window is open
        while (UIManager.inst.terminal_hackinfoArea1.activeInHierarchy)
        {
            yield return null;
        }

        // And play the sound
        AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_ui["ACCESS"]); // (UI - ACCESS)

        switch (id)
        {
            case 0: // Fabricators
                foreach (var M in MapManager.inst.machines_fabricators)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                break;
            case 1: // Garrisons
                foreach (var M in MapManager.inst.machines_garrisons)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                break;
            case 2: // Machines (all interactable)
                foreach (var M in MapManager.inst.machines_fabricators)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                foreach (var M in MapManager.inst.machines_garrisons)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                foreach (var M in MapManager.inst.machines_recyclingUnits)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                foreach (var M in MapManager.inst.machines_repairStation)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                foreach (var M in MapManager.inst.machines_scanalyzers)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                foreach (var M in MapManager.inst.machines_terminals)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                break;
            case 3: // Recycling Units
                foreach (var M in MapManager.inst.machines_recyclingUnits)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                break;
            case 4: // Repair Stations
                foreach (var M in MapManager.inst.machines_repairStation)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                break;
            case 5: // Scanalyzers
                foreach (var M in MapManager.inst.machines_scanalyzers)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                break;
            case 6: // Terminals
                foreach (var M in MapManager.inst.machines_terminals)
                {
                    GameManager.inst.RevealWorldMachine(M.GetComponentInChildren<MachinePart>().gameObject);
                }
                break;
        }
    }

    public void RevealWorldMachine(GameObject specificMachine = null, MachineType type = MachineType.Misc)
    {
        if(specificMachine != null) // Reveal this specific machine
        {
            specificMachine.GetComponent<MachinePart>().RevealMe();
        }
        else // Reveal a random machine based on type
        {
            MachinePart m = null;
            switch (type)
            {
                case MachineType.Fabricator:
                    m = MapManager.inst.machines_fabricators[Random.Range(0, MapManager.inst.machines_fabricators.Count - 1)].GetComponentInChildren<MachinePart>();
                    if (m.parentPart.isExplored) // We already know this one, roll again
                    {
                        GameManager.inst.RevealWorldMachine(null, type);
                    }
                    else
                    {
                        m.parentPart.RevealMe();
                    }
                    break;
                case MachineType.Garrison:
                    m = MapManager.inst.machines_garrisons[Random.Range(0, MapManager.inst.machines_garrisons.Count - 1)].GetComponentInChildren<MachinePart>();
                    if (m.parentPart.isExplored) // We already know this one, roll again
                    {
                        GameManager.inst.RevealWorldMachine(null, type);
                    }
                    else
                    {
                        m.parentPart.RevealMe();
                    }
                    break;
                case MachineType.Recycling:
                    m = MapManager.inst.machines_recyclingUnits[Random.Range(0, MapManager.inst.machines_recyclingUnits.Count - 1)].GetComponentInChildren<MachinePart>();
                    if (m.parentPart.isExplored) // We already know this one, roll again
                    {
                        GameManager.inst.RevealWorldMachine(null, type);
                    }
                    else
                    {
                        m.parentPart.RevealMe();
                    }
                    break;
                case MachineType.RepairStation:
                    m = MapManager.inst.machines_repairStation[Random.Range(0, MapManager.inst.machines_repairStation.Count - 1)].GetComponentInChildren<MachinePart>();
                    if (m.parentPart.isExplored) // We already know this one, roll again
                    {
                        GameManager.inst.RevealWorldMachine(null, type);
                    }
                    else
                    {
                        m.parentPart.RevealMe();
                    }
                    break;
                case MachineType.Scanalyzer:
                    m = MapManager.inst.machines_scanalyzers[Random.Range(0, MapManager.inst.machines_scanalyzers.Count - 1)].GetComponentInChildren<MachinePart>();
                    if (m.parentPart.isExplored) // We already know this one, roll again
                    {
                        GameManager.inst.RevealWorldMachine(null, type);
                    }
                    else
                    {
                        m.parentPart.RevealMe();
                    }
                    break;
                case MachineType.Terminal:
                    m = MapManager.inst.machines_terminals[Random.Range(0, MapManager.inst.machines_terminals.Count - 1)].GetComponentInChildren<MachinePart>();
                    if (m.parentPart.isExplored) // We already know this one, roll again
                    {
                        GameManager.inst.RevealWorldMachine(null, type);
                    }
                    else
                    {
                        m.parentPart.RevealMe();
                    }
                    break;
                case MachineType.CustomTerminal:
                    m = MapManager.inst.machines_customTerminals[Random.Range(0, MapManager.inst.machines_customTerminals.Count - 1)].GetComponentInChildren<MachinePart>();
                    if (m.parentPart.isExplored) // We already know this one, roll again
                    {
                        GameManager.inst.RevealWorldMachine(null, type);
                    }
                    else
                    {
                        m.parentPart.RevealMe();
                    }
                    break;
                case MachineType.DoorTerminal:
                    break;
                case MachineType.Misc:
                    break;
            }
        }
    }

    public List<char> storedIntel = new List<char>(); // TODO: Change this to appropriate variables later

    /// <summary>
    /// Acts on (and reveals) any stored intel collected on previous levels. Called on the start of every (non-branch) floor.
    /// </summary>
    public void RevealStoredIntel() // TODO
    {
        if(storedIntel.Count > 0)
        {
            // Play the sound
            AudioManager.inst.CreateTempClip(PlayerData.inst.transform.position, AudioManager.inst.dict_ui["ACCESS"]); // (UI - ACCESS)

            // Merge all intel into types
            // - Zones (areas)
            // - Access points
            // - (ALL) Machines OR Individual machine groups
            // - Traps
            // - Access points

            // Do a *blue* log message for each group

            string message = "";
            UIManager.inst.CreateNewLogMessage(message, UIManager.inst.deepInfoBlue, UIManager.inst.infoBlue, false, true);
        }
    }
    #endregion

    #endregion

    #region Squad Deployment

    public void DeploySquadTo(string type, InteractableMachine targetLocation) // ------------ NOT FINISHED ---------
    {
        // We want to deploy a squad of a specific type of bots to go to a location.


        switch (type)
        {
            case "Investigation":

                break;
            case "Extermination":

                break;
            case "Programmer":

                break;
            case "Demolisher":

                break;
            // more after this
        }
    }

    #endregion

    public void CreateMeleeAttackIndicator(Vector2Int pos, float rotation, Item weapon, bool hit = true)
    {
        var go = Instantiate(UIManager.inst.prefab_meleeIndicator, new Vector3(pos.x, pos.y), Quaternion.identity); // Instantiate
        go.name = $"Melee Indicator: {pos.x},{pos.y}"; // Give grid based name

        if(PlayerData.inst)
            go.transform.parent = PlayerData.inst.transform;

        go.GetComponent<MeleeAttackIndicator>().Init(weapon, rotation, hit); // Init
    }

    public void MachineTimerUpdate() // Do a production check on any working machines
    {
        foreach (GameObject obj in MapManager.inst.machines_fabricators)
        {
            if (obj.GetComponentInChildren<Fabricator>().working)
            {
                obj.GetComponentInChildren<Fabricator>().Check();
            }
        }

        foreach (GameObject obj in MapManager.inst.machines_scanalyzers)
        {
            if (obj.GetComponentInChildren<Scanalyzer>().working)
            {
                obj.GetComponentInChildren<Scanalyzer>().Check();
            }
        }

        foreach (GameObject obj in MapManager.inst.machines_repairStation)
        {
            if (obj.GetComponentInChildren<RepairStation>().working)
            {
                obj.GetComponentInChildren<RepairStation>().Check();
            }
        }

        foreach (GameObject obj in MapManager.inst.machines_recyclingUnits)
        {
            if (obj.GetComponentInChildren<RecyclingUnit>().working)
            {
                obj.GetComponentInChildren<RecyclingUnit>().Check();
            }
        }
    }

    public void GrantSchematicKnowledge(ItemObject item = null, BotObject bot = null)
    {
        if(item != null)
        {
            item.schematicDetails.hasSchematic = true;
        }
        else if(bot != null)
        {
            bot.schematicDetails.hasSchematic = true;
        }
    }

    public ColorPallet colors;
}

#region Global Actions
[System.Serializable]
/// <summary>
/// Used mainly for hack rewards.
/// -Recall patrols
/// -Lower alert status
/// -Locate traps
/// etc...
/// </summary>
public class GlobalActions
{
    public TerminalCommandType type;
    public GameObject connectedMachine;

    public GlobalActions(TerminalCommandType type, string specifier, GameObject connectedMachine = null, ItemObject item = null, BotObject bot = null)
    {
        this.type = type;
        this.connectedMachine = connectedMachine;

    }
}
#endregion

[System.Serializable]
public class ColorPallet
{
    [Header("Machine Colors")]
    public Color machine_terminal;
    public Color machine_garrison;
    public Color machine_recycler;
    public Color machine_fabricator;
    public Color machine_scanalyzer;
    public Color machine_repairbay;
    public Color machine_customterminal;
    public Color machine_static;

    [Header("Bottom Message Colors")]
    public Color bm_blue_dark;
    public Color bm_blue_norm;
    public Color bm_blue_text;
    //
    public Color bm_green_dark;
    public Color bm_green_norm;
    public Color bm_green_text;
    //
    public Color bm_red_dark;
    public Color bm_red_norm;
    public Color bm_red_text;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Unity.Collections;

/// <summary>
/// Used for directly typing out commands during hacking.
/// </summary>
public class UIHackInputfield : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI primaryText;
    public TextMeshProUGUI backerText; // The black highlight
    public TextMeshProUGUI main_backerText; // The black highlight
    [Tooltip("The gray-er text which auto-updates to the closest possible command to what is being typed.")]
    public TextMeshProUGUI suggestionText;
    public TMP_InputField field;

    [Header("Color Pallet")]
    public Color fadedGray;
    public Color headerWhite;
    public Color darkGreenColor;
    public Color grayedOutColor;
    public Color lowDetColor;
    public Color mediumDetColor;
    public Color highDetColor;
    public Color veryHighDetColor;

    [Header("SuggestionBox")]
    public GameObject box_main;
    public GameObject box_area;
    public GameObject box_AC_prefab;
    [Tooltip("The counterpart to the ordinary suggestion text. Only active when the Suggestions Box is open.")]
    public TextMeshProUGUI box_suggestionText;
    public GameObject box_small;

    [Header("Audio")]
    public AudioSource aSource;
    private AudioClip aClip;
    [SerializeField] private Vector2 pitchRange;

    public void Setup()
    {
        // The text appears with no fanfare, so no need for a fancy animation

        // However, if the player has any custom codes, a related window needs to open.
        // So we will be animating that instead.
        // > The window will open next to where the input field is.

        if(PlayerData.inst.customCodes.Count > 0)
        {
            OpenCodesWindow();
        }

        aClip = AudioManager.inst.dict_ui["KEYIN"]; // UI - KEYIN

        field.onValueChanged.AddListener(OnInputFieldValueChanged);
        field.onSubmit.AddListener(TrySubmit);
        AC_Setup();
        SetFocus(true);
    }

    #region Codes Window

    public void OpenCodesWindow()
    {
        // We want to aling the codes window with the input field (this).
        Vector3[] v = new Vector3[4];
        this.GetComponent<RectTransform>().GetWorldCorners(v);

        UIManager.inst.Terminal_OpenCodes(v[1].y);
    }

    public void CloseCodesWindow()
    {
        UIManager.inst.Terminal_CloseCodes();
    }

    #endregion

    /// <summary>
    /// Called from *PlayerGridMovement.cs* when the ESCAPE bind is pressed.
    /// </summary>
    public void Input_Escape()
    {
        if (field.isFocused)
        {
            CloseCodesWindow();
            // and destroy this manual input
            UIManager.inst.terminal_activeIField = null;
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Called from *PlayerGridMovement.cs* when the TAB (Autocomplete) bind is pressed.
    /// </summary>
    public void Input_Tab()
    {
        if (field.isFocused)
        {
            // -- Just here to be safe --
            if (field.text.Length == 0 && UIManager.inst.terminal_manualBuffer.Count > 0)
            {
                BufferSuggestions();
            }

            SuggestionBoxCheck();

            SetFocus(true);

            string hackString = "";

            if (box_main.activeInHierarchy)
            {
                hackString = HF.GetLeftSubstring(field.text) + "(" + box_suggestionText.text;

                field.textComponent.text = hackString;
                field.text = hackString;

                field.caretPosition = field.text.Length; // Set to end of text

                // And we want to close the window
                ClearSuggestions();
                CloseCodesWindow();
            }
            else
            {
                hackString = suggestionText.text;

                field.textComponent.text = hackString;
                field.text = hackString;

                field.caretPosition = field.text.Length; // Set to end of text
            }

            // Need to call this again or else the auto-suggest breaks.
            SuggestionBoxCheck();
        }

        ForceMeshUpdates();
    }

    private void ChooseBoxSuggestions()
    {
        if (box_main.activeInHierarchy)
        {
            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                if (suggestionID < (activeSuggestions.Count - 1))
                {
                    // Navigate down
                    suggestionID++;
                    currentSuggestion = suggestions[suggestionID];
                    box_suggestionText.text = currentSuggestion + ")";
                    IndicateSelectedSuggestion(suggestionID);
                }
            }
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                if (suggestionID != 0)
                {
                    // Navigate up
                    suggestionID--;
                    currentSuggestion = suggestions[suggestionID];
                    box_suggestionText.text = currentSuggestion + ")";
                    IndicateSelectedSuggestion(suggestionID);
                }
            }
        }
    }

    private void Update()
    {
        SetFocus(true);
        field.caretPosition = field.text.Length;

        if (field.isFocused)
        {
            BackerTextUpdate();
            ForceMeshUpdates();

            // Previous Suggestions check
            bool EMPTYFIELD = field.text.Length == 0;
            if ((EMPTYFIELD || !box_main.gameObject.activeInHierarchy) && UIManager.inst.terminal_manualBuffer.Count > 0)
            {
                if (EMPTYFIELD) { suggID = -1; }

                BufferSuggestions();
            }
        }
        
        // Suggestion box navigation
        ChooseBoxSuggestions();

        SetFocus(true);
        field.caretPosition = field.text.Length;
    }

    public void TrySubmit(string input)
    {
        field.caretPosition = field.text.Length;
        suggID = -1;
        // Check if the current string is a valid code
        string attempt = field.text;
        TerminalCommand command;
        HackObject hack;
        (hack, command) = HF.ParseHackString(attempt);

        UIManager.inst.terminal_manualBuffer.Add(new KeyValuePair<string, TerminalCommand>(attempt, command)); // Add to buffer

        if (hack != null)
        {
            AttemptHack(hack, command);
        }
        else // Invalid command? PUNISHED!!
        { // This is in uppercase for stylistic reasons.
            UIManager.inst.Terminal_CreateResult("Unknown command.", highDetColor, (">>" + field.text.ToUpper()), true, 0.5f);
        }

        // Lastly, we destroy this input field
        ShutDown();
    }

    public void AttemptHack(HackObject hack, TerminalCommand command)
    {
        // -- Calculate Chance of Success --
        float chance = 0f;

        int secLvl = UIManager.inst.terminal_targetTerm.secLvl;
        if (secLvl == 0)
        {
            chance = 1f; // If its an open system we auto-succeed
        }
        else
        {
            float baseChance = 1f;
            if (secLvl == 1) // We are using direct chance because indirect is done somewhere else
            {
                baseChance = (float)((float)hack.directChance.x / 100f);
            }
            else if (secLvl == 2)
            {
                baseChance = (float)((float)hack.directChance.y / 100f);
            }
            else if (secLvl == 3)
            {
                baseChance = (float)((float)hack.directChance.z / 100f);
            }
            chance = HF.CalculateHackSuccessChance(baseChance);
        }

        string printoutFill = "";
        if(command != null)
        {
            if(command.item != null)
            {
                printoutFill = command.item.itemName;
            }
            else if(command.bot != null)
            {
                printoutFill = command.bot.botName;
            }
            else if(command.knowledge != null)
            {
                printoutFill = command.knowledge.name;
            }
        }

        float random = Random.Range(0.0f, 1.0f);

        // We should do a special check here in case the specified command is not valid given the current machine (in cases of Manual Entry).
        // For example, they should not be able to 'Enumerate(Guards)' on a Scanalyzer, since that command only works on terminals.
        if (validHacks.Contains(hack))
        {
            if (hack.hackType == TerminalCommandType.Query
            || hack.hackType == TerminalCommandType.Schematic
            || hack.hackType == TerminalCommandType.Analysis
            || hack.hackType == TerminalCommandType.Prototypes)
            {
                if (MapManager.inst.centerDatabaseLockout) // Instant fail
                {
                    UIManager.inst.Terminal_CreateResult("Central database compromised, local access revoked.", highDetColor, (">>" + HF.HackToPrintout(hack, printoutFill)), true, 0.7f);
                    return;
                }
            }

            if (random <= chance) // SUCCESS
            {
                // ---
                // ** Central Database Lockout **
                // "Successful indirect hacking of central "database-related" targets (queries, schematics, analysis, prototypes)
                // incurs a 25% chance to trigger a database lockout, preventing indirect access to those types of targets at every terminal on the same map."
                // ---

                List<float> bonuses = HF.SystemShieldBonuses();

                if (Random.Range(0.0f, 1.0f) <= (0.25f - bonuses[3])
                    && (hack.hackType == TerminalCommandType.Query
                || hack.hackType == TerminalCommandType.Schematic
                || hack.hackType == TerminalCommandType.Analysis
                || hack.hackType == TerminalCommandType.Prototypes))
                {
                    MapManager.inst.centerDatabaseLockout = true;

                    UIManager.inst.Terminal_CreateResult("Central database compromised, local access revoked.", highDetColor, (">>" + HF.HackToPrintout(hack, printoutFill)), true, 0.75f);
                    UIManager.inst.CreateNewLogMessage("Central database lockdown, local access denied.", UIManager.inst.complexWhite, UIManager.inst.inactiveGray, true, true);
                    UIManager.inst.CreateLeftMessage("ALERT: Central database lockdown, local access denied.", 10f, AudioManager.inst.dict_game["DISPATCH_ALERT"]); // GAME - DISPATCH_ALERT
                    return;
                }

                // ---

                SucceedHack(printoutFill, hack, command);
            }
            else // FAILURE
            {
                UIManager.inst.Terminal_CreateResult(HF.GenericHackFailure(random), highDetColor, (">>" + HF.HackToPrintout(hack, printoutFill)), true, chance - random);
            }
        }
        else // This hack cannot be ran on this specific machine. Give a special warning.
        {
            UIManager.inst.Terminal_CreateResult("Command unavailable on this system.", highDetColor, (">>" + HF.HackToPrintout(hack, printoutFill).ToLower()), true, 0.4f);
        }

        
        HF.ScrollToBottom(UIManager.inst.terminal_resultsScrollrect); // Force scroll to bottom
    }

    private void SucceedHack(string fill, HackObject hack, TerminalCommand command)
    {
        string header = ">>" + HF.HackToPrintout(hack, fill);
        string rewardString = HF.MachineReward_PrintPLUSAction(command, command.item, command.bot);

        if (rewardString.Length > 0)
        {
            // Create result in terminal
            UIManager.inst.Terminal_CreateResult(rewardString, lowDetColor, header, true);
            // Create log messages
            UIManager.inst.CreateNewLogMessage(header, lowDetColor, darkGreenColor, true);
            UIManager.inst.CreateNewLogMessage(rewardString, UIManager.inst.deepInfoBlue, UIManager.inst.infoBlue, true, true);
        }
    }

    #region Auto-Suggestion

    /*
     * -IMPORTANT NOTE-
     * If the amount of suggestions is greater than our specified value (maxSuggestions), 
     * then we DONT open the box, and instead do a shorter autosuggest that also contains
     * a (##+) at the end to how how many others there are. With arrow keys switching between.
     */

    [Tooltip("A string list of all valid hacks for the currently open machine. Contains only the first half of the hack. Ex: Enumerate(Guards) -> enumerate.")]
    public List<string> AC_list = new List<string>();
    [Tooltip("A list of all valid hacks for the currently open machine. Compiled in Setup() -> AC_Setup() upon startup.")]
    public List<HackObject> validHacks = new List<HackObject>();
    private void AC_Setup()
    {
        AC_list.Clear();

        foreach (HackObject hack in MapManager.inst.hackDatabase.Hack)
        {
            // Hack must be compatible with this machine.
            if(UIManager.inst.terminal_targetTerm.type == hack.relatedMachine)
            {
                AC_list.Add(HF.GetLeftSubstring(HF.ParseHackName(hack).ToLower()));
                validHacks.Add(hack);
            }
        }

        AC_list = AC_list.Distinct().ToList(); // Remove duplicates
    }


    private void OnInputFieldValueChanged(string value) // Place a sound every time a character is altered
    {
        SuggestionBoxCheck();

        // Sounds
        aSource.pitch = Random.Range(pitchRange.x, pitchRange.y); // Randomize pitch so it sounds distinct each time
        aSource.PlayOneShot(aClip, 1f);

        // Text update
        if (string.IsNullOrEmpty(value))
        {
            suggestionText.text = string.Empty;
            return;
        }

        // Suggestion text update
        if (!field.text.Contains("("))
        {
            string closestMatch = GetClosestMatch(value, AC_list);
            suggestionText.text = closestMatch;
        }
    }

    private string GetClosestMatch(string value, List<string> options)
    {
        string closestMatch = string.Empty;
        int closestDistance = int.MaxValue;

        foreach (string command in options)
        {
            if (command.StartsWith(value))
            {
                int distance = command.Length - value.Length;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMatch = command;
                }
            }
        }

        return closestMatch;
    }

    int suggID = -1;
    /// <summary>
    /// Allows the user to cycle through the suggestions in the terminal buffer (previous commands).
    /// </summary>
    private void BufferSuggestions()
    {
        List<KeyValuePair<string, TerminalCommand>> _suggestions = UIManager.inst.terminal_manualBuffer;

        if (Keyboard.current.downArrowKey.wasPressedThisFrame) // We want to go from older -> newer
        {

            if (suggID > 0) // current ID isn't 0 (we can still go lower, aka newer)
            {
                if (_suggestions[0].Value == UIManager.inst.terminal_manualBuffer[0].Value) // Need to make sure the list is reversed
                    _suggestions.Reverse();

                suggID--; // Go down an ID (--> newer)
                field.text = _suggestions[suggID].Key; // Update field
            }
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame) // We want to go from newer -> older
        {
            if (suggID < (_suggestions.Count - 1)) // There are still older commands than the current (suggested) one
            {
                if (_suggestions[0].Value == UIManager.inst.terminal_manualBuffer[0].Value) // Need to make sure the list is reversed
                    _suggestions.Reverse();

                suggID++; // Go up an ID (--> older)
                field.text = _suggestions[suggID].Key; // Update field
            }
        }
    }

    [SerializeField] private List<GameObject> activeSuggestions = new List<GameObject>();
    [SerializeField] private List<string> suggestions = new List<string>();
    [SerializeField] private string currentSuggestion = "";
    [SerializeField] private int suggestionID = -1;
    [SerializeField] private bool filled = false;
    private void SuggestionBoxCheck()
    {
        string fieldString = field.text;

        // -- Micro Suggestions Check --
        #region Micro-suggestions (non-box)
        List<string> microSuggestions = new List<string>();

        foreach (HackObject hack in validHacks)
        {
            string hackName = hack.name.ToLower();

            if (!hack.doNotSuggest && hackName.Contains(fieldString)) // If this command is a variant of what the player is currently attempting to type
            {
                microSuggestions.Add(hack.name);
            }
        }

        microSuggestions = microSuggestions.Distinct().ToList(); // Remove duplicates

        if(microSuggestions.Count >= GlobalSettings.inst.maxHackingSuggestions) // Too many suggestions, do the simplified version
        {
            ClearSuggestions();
        }
        #endregion
        else // Not that many suggestions, use the window
        {
            /*  -- How the Suggestion Box works --
             * At the bare minimum, the suggestions box should only be open if:
             *  1. There is text on the field
             *  2. The "(" character is on the field
             *  3. The ")" character is NOT on the field
             *  
             * The box, its suggestion text, and options needs to be updated dynamically on every input change.
             *  -We should only should the box if there are more than 1 suggestion to show
             *  -If the box is open, the nearest valid command should come first in the list and be highlighted in green.
             *  -The suggestion text needs to auto-update to the nearest valid command
             */

            bool CANSHOWBOX = field.text.Length > 0 && field.text.Contains("(") && field.text[field.text.Length - 1].ToString() != ")";
            if (CANSHOWBOX)
            {
                // Reset the suggestions list
                suggestions.Clear();

                // Clean up the box window to remove duplicates
                HideSuggestionsBox();
                ClearSuggestions();

                // Fill it
                LoadBoxSuggestions();

                // NOTE: The main elements of interest here are `suggestionText` and `box_suggestionText`
                //                                             >>primary_suggestion___(box_suggestion

                // More than 1?
                if (suggestions.Count > 1) // We need to show the box
                {
                    // Is there anything beyond the "(" character?
                    string[] inputsplit = field.text.Split("(");
                    if(inputsplit.Length > 1 && inputsplit[1] != null && inputsplit[1] != "") // There is, we should suggest the closest matching one
                    {
                        string bestMatch = GetClosestMatch(inputsplit[1], suggestions);
                        suggestionID = suggestions.IndexOf(bestMatch);

                        // Failsafe
                        if(suggestionID == -1)
                        {
                            suggestionID = 0;
                        }

                        currentSuggestion = suggestions[suggestionID];
                    }
                    else // There isn't, we should just suggest the first one
                    {
                        suggestionID = 0;
                        currentSuggestion = suggestions[suggestionID];
                    }

                    box_suggestionText.text = currentSuggestion + ")";
                    suggestionText.text = inputsplit[0] + "(";

                    #region Suggestions Box
                    // Open the window
                    ShowSuggestionsBox();

                    // Now instantiate a list of them
                    foreach (string command in suggestions)
                    {
                        GameObject code = Instantiate(box_AC_prefab, box_area.transform.position, Quaternion.identity);
                        code.transform.SetParent(box_area.transform);
                        code.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                        // Add it to list
                        activeSuggestions.Add(code);
                        // Assign Details
                        TextMeshProUGUI[] textUGUIs = code.GetComponentsInChildren<TextMeshProUGUI>();
                        textUGUIs[1].text = command; // We don't want the fill text
                    }

                    // Highlight the first suggestion in the list
                    IndicateSelectedSuggestion(suggestionID);

                    StartCoroutine(SBoxPositionLateUpdate());
                    #endregion
                }
                else if (suggestions.Count == 1) // One
                {
                    // Remove the box
                    HideSuggestionsBox();

                    // Just rely on `suggestionText`
                    currentSuggestion = suggestions[0];
                    suggestionText.text = field.text.Split("(")[0] + "(" + currentSuggestion + ")";
                }
                else // No suggestions
                {
                    // Remove the box
                    HideSuggestionsBox();

                    // Just set suggestions to current text
                    suggestionText.text = field.text; //field.text.Split("(")[0];
                }

            }
            else
            {
                // Remove the box
                HideSuggestionsBox();
            }
        }

        ForceMeshUpdates();
    }

    private void IndicateSelectedSuggestion(int id)
    {
        // Gray (dark green) them all out.
        foreach (GameObject item in activeSuggestions.ToList())
        {
            TextMeshProUGUI[] textUGUIs2 = item.GetComponentsInChildren<TextMeshProUGUI>();
            textUGUIs2[1].color = darkGreenColor; // We don't want the fill text
            item.GetComponentInChildren<TextMeshProUGUI>().color = darkGreenColor;
        }

        // Indicate the selected one
        TextMeshProUGUI[] textUGUIs = activeSuggestions[id].GetComponentsInChildren<TextMeshProUGUI>();
        textUGUIs[1].color = lowDetColor; // We don't want the fill text
    }

    private void LoadBoxSuggestions()
    {
        int floorRating = MapManager.inst.currentLevel + 11; // ex: -10 + 11 = 1
        foreach (HackObject hack in validHacks)
        {
            if (hack.name.Contains("(")) // Hacks must have ( in them to be in the suggestions box.
            {
                if (!hack.doNotSuggest)
                {
                    string hackName = hack.name.ToLower();
                    string[] split = field.text.ToLower().Split("(");
                    bool additional = split.Length > 1 && split[1] != null && split[1] != ""; // We want to parse to include beyond the "("

                    // If this command is a variant of what the player is currently attempting to type
                    if ((additional && hackName.Contains(split[0]) && hackName.Contains(split[1])) || (!additional && hackName.Contains(split[0])))
                    {
                        // We also want to fill the (inside) if its a certain type that grants knowledge like:
                        // -Query(Knowledge), Analysis(Bot), Schematic(Item), Schematic(Bot)
                        if (hack.hackType == TerminalCommandType.Query)
                        {
                            // No suggestions for lore!
                            /*
                            foreach (KnowledgeObject lore in MapManager.inst.knowledgeDatabase.Data)
                            {
                                if (!lore.knowByPlayer)
                                {
                                    newCom = lore.name;
                                }
                            }
                            */
                        }
                        else if (hack.hackType == TerminalCommandType.Analysis) // Bots
                        {
                            // Want to make sure its the same tier (and player doesn't have data on it)
                            int rating = -1;
                            bool p = false;
                            (rating, p) = HF.GetTierAndP(hack.name);

                            foreach (BotObject bot in MapManager.inst.botDatabase.Bots)
                            {
                                if (bot.tier == rating && !bot.playerHasAnalysisData && bot.tier >= floorRating)
                                {
                                    suggestions.Add(bot.name);
                                    // We remove duplicates later so this shouldn't be an issue
                                }
                            }
                        }
                        else if (hack.hackType == TerminalCommandType.Schematic)
                        {
                            // Bots
                            // Want to make sure its the same tier (and player doesn't have data on it)
                            int rating = -1;
                            bool p = false;
                            (rating, p) = HF.GetTierAndP(hack.name);

                            foreach (BotObject bot in MapManager.inst.botDatabase.Bots)
                            {
                                if (bot.tier == rating && !bot.playerHasAnalysisData && bot.tier >= floorRating)
                                {
                                    suggestions.Add(bot.name);
                                    // We remove duplicates later so this shouldn't be an issue
                                }
                            }

                            // Items
                            foreach (ItemObject item in MapManager.inst.itemDatabase.Items)
                            {
                                if (item.rating == rating && !item.knowByPlayer && item.star == p && item.rating >= floorRating)
                                {
                                    suggestions.Add(item.itemName);
                                    // We remove duplicates later so this shouldn't be an issue
                                }
                            }
                        }
                        else
                        {
                            string newCom = HF.ExtractText(hack.name);
                            suggestions.Add(newCom);
                        }
                    }
                }
            }
        }

        suggestions = suggestions.Distinct().ToList(); // Remove duplicates
    }

    private void ShowSuggestionsBox()
    {
        box_main.SetActive(true);
        box_small.SetActive(true);

        BackerTextUpdate();

        SBoxPositionUpdate();
    }

    private void SBoxPositionUpdate()
    {
        if (box_main.gameObject.activeInHierarchy)
        {
            // Set the positions correctly (we are basing it off of the top right corner of the field.textComponent black backer
            Vector3[] v = new Vector3[4];
            backerText.GetComponent<RectTransform>().GetWorldCorners(v);
            float currentX = v[2].x - sbox_offset;

            box_main.transform.position = new Vector3(currentX, box_main.transform.position.y, 0);
            box_small.transform.position = new Vector3(currentX, box_small.transform.position.y, 0);
        }
    }

    private IEnumerator SBoxPositionLateUpdate()
    {
        // I truly do dislike having to do this, but its better than throwing it in Update i guess.
        // The reason this is required is because (it seems) the forceful mesh updates aren't as "instant" as they claim.

        yield return null;

        SBoxPositionUpdate();
    }

    [SerializeField] private float sbox_offset = 4.5f;
    private void BackerTextUpdate()
    {
        backerText.text = "<mark=#000000>>>" + suggestionText.text + "</mark>"; // Mark highlights it as pure black
        main_backerText.text = "<mark=#000000>" + field.text + "</mark>"; // Mark highlights it as pure black

        backerText.ForceMeshUpdate();
        main_backerText.ForceMeshUpdate();
    }

    private void HideSuggestionsBox()
    {
        // Clear active suggestions
        foreach (GameObject item in activeSuggestions.ToList())
        {
            Destroy(item);
        }
        activeSuggestions.Clear();

        // Close the window
        box_main.SetActive(false);
        box_small.SetActive(false);
    }

    private void ClearSuggestions()
    {
        currentSuggestion = "";
        box_suggestionText.text = "";
        suggestionID = -1;
    }

    #endregion

    public void SetFocus(bool active)
    {
        
        if(active)
        {
            field.Select();
            field.ActivateInputField();
        }
        else
        {
            field.ReleaseSelection();
            field.DeactivateInputField();
        }
        
    }

    public void ShutDown()
    {
        SetFocus(false);
        Destroy(this.gameObject);
    }

    public void ForceMeshUpdates()
    {
        primaryText.ForceMeshUpdate();
        backerText.ForceMeshUpdate();
        suggestionText.ForceMeshUpdate();
        main_backerText.ForceMeshUpdate();
        field.textComponent.ForceMeshUpdate();
    }
}

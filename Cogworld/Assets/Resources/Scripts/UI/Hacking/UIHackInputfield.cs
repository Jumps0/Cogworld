using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

/// <summary>
/// Used for directly typing out commands during hacking.
/// </summary>
public class UIHackInputfield : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI primaryText;
    public TextMeshProUGUI backerText; // The black highlight
    public TextMeshProUGUI main_backerText; // The black highlight
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
    public TextMeshProUGUI box_suggestionText;
    public GameObject box_small;

    [Header("Audio")]
    public AudioSource aSource;
    public AudioClip aClip;

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
                field.textComponent.ForceMeshUpdate();

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
                field.textComponent.ForceMeshUpdate();

                field.caretPosition = field.text.Length; // Set to end of text
            }
        }
    }

    private void Update()
    {
        SetFocus(true);
        field.caretPosition = field.text.Length;

        if (field.isFocused)
        {
            /*
            if(field.text.Length == 0 && UIManager.inst.terminal_manualBuffer.Count > 0) 
            {
                BufferSuggestions();
            }

            SuggestionBoxCheck();
            */

            backerText.text = "<mark=#000000>>>" + suggestionText.text + "</mark>"; // Mark highlights it as pure black
            main_backerText.text = "<mark=#000000>" + field.text + "</mark>"; // Mark highlights it as pure black
            primaryText.ForceMeshUpdate();
            backerText.ForceMeshUpdate();
            suggestionText.ForceMeshUpdate();
            main_backerText.ForceMeshUpdate();
        }

        SetFocus(true);
        field.caretPosition = field.text.Length;
    }

    int suggID = -1;
    private void BufferSuggestions()
    {
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (suggID > 0)
            {
                suggID--;
                field.text = UIManager.inst.terminal_manualBuffer[suggID].Key;
            }
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            if(suggID < (UIManager.inst.terminal_manualBuffer.Count - 1))
            {
                suggID++;
                field.text = UIManager.inst.terminal_manualBuffer[suggID].Key;
            }
        }
    }

    public void TrySubmit(string input)
    {
        field.caretPosition = field.text.Length;
        // Check if the current string is a valid code
        string attempt = field.text;
        TerminalCommand command;
        HackObject hack;
        (hack, command) = HF.ParseHackString(attempt);

        UIManager.inst.terminal_manualBuffer.Add(new KeyValuePair<string, TerminalCommand>(attempt, command)); // Add to buffer

        if (hack != null)
        {
            AttemptHack(hack, command);
            // Destroy this input field
            SetFocus(false);
            Destroy(this.gameObject);
        }
    }

    public void AttemptHack(HackObject hack, TerminalCommand command)
    {
        Debug.Log($"Attempting hack: {hack} | {hack.trueName} | C: {command}");

        // -- Calculate Chance of Success --
        float chance = 0f;
        bool openSystem = false;
        int secLvl = HF.GetMachineSecLvl(UIManager.inst.terminal_targetTerm);
        if (secLvl == 0)
        {
            chance = 1f; // Open System
            openSystem = true;
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
            Debug.Log($"Base chance: {baseChance} | Chance: {chance}");
        }

        float random = Random.Range(0.0f, 1.0f);
        Debug.Log($"Chance: {chance} | Random: {random} | s?{random <= chance}");
        if (hack.hackType == TerminalCommandType.Query 
            || hack.hackType == TerminalCommandType.Schematic 
            || hack.hackType == TerminalCommandType.Analysis 
            || hack.hackType == TerminalCommandType.Prototypes)
        {
            if (MapManager.inst.centerDatabaseLockout) // Instant fail
            {
                UIManager.inst.Terminal_CreateResult("Central database compromised, local access revoked.", highDetColor, (">>" + HF.ParseHackName(hack)), true);
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

                UIManager.inst.Terminal_CreateResult("Central database compromised, local access revoked.", highDetColor, (">>" + HF.ParseHackName(hack)), true);
                UIManager.inst.CreateNewLogMessage("Central database lockdown, local access denied.", UIManager.inst.complexWhite, UIManager.inst.inactiveGray, true, true);
                UIManager.inst.CreateLeftMessage("ALERT: Central database lockdown, local access denied.", 10f, AudioManager.inst.dict_game["DISPATCH_ALERT"]); // GAME - DISPATCH_ALERT
                return;
            }

            // ---

            string right = HF.GetRightSubstring(field.text); // inside)
            right = right.Substring(0, right.Length - 1); // Remove the ")", now: inside

            string header = ">>" + HF.ParseHackName(hack, right);
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
        else // FAILURE
        {
            UIManager.inst.Terminal_CreateResult(HF.GenericHackFailure(random), highDetColor, (">>" + HF.ParseHackName(hack)), true);
        }
        HF.ScrollToBottom(UIManager.inst.terminal_resultsScrollrect); // Force scroll to bottom
    }

    #region Auto-Suggestion

    /*
     * -IMPORTANT NOTE-
     * 
     * If the amount of suggestions is greater than (lets just say 8), 
     * then we DONT open the box, and instead do a shorter autosuggest
     * that also contains a (##+) at the end to how how many others
     * there are. With arrow keys switching between.
     */

    public List<string> AC_list = new List<string>();
    private void AC_Setup()
    {
        AC_list.Clear();

        foreach (HackObject hack in MapManager.inst.hackDatabase.Hack)
        {
            AC_list.Add(HF.GetLeftSubstring(HF.ParseHackName(hack).ToLower()));
        }

        AC_list = AC_list.Distinct().ToList(); // Remove duplicates
    }


    private void OnInputFieldValueChanged(string value)
    {
        aSource.PlayOneShot(aClip, 1f);
        if (string.IsNullOrEmpty(value))
        {
            suggestionText.text = string.Empty;
            return;
        }

        string closestMatch = GetClosestMatch(value);
        suggestionText.text = closestMatch;
    }

    private string GetClosestMatch(string value)
    {
        string closestMatch = string.Empty;
        int closestDistance = int.MaxValue;

        foreach (string command in AC_list)
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

    // Calculates the Levenshtein distance between two strings
    private int LevenshteinDistance(string source, string target)
    {
        int[,] matrix = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; i++)
        {
            matrix[i, 0] = i;
        }

        for (int j = 0; j <= target.Length; j++)
        {
            matrix[0, j] = j;
        }

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1;

                matrix[i, j] = Mathf.Min(
                    matrix[i - 1, j] + 1,
                    matrix[i, j - 1] + 1,
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        return matrix[source.Length, target.Length];
    }

    public List<GameObject> activeSuggestions = new List<GameObject>();
    public List<string> suggestions = new List<string>();
    public string currentSuggestion = "";
    public int suggestionID = -1;
    private bool filled = false;
    private void SuggestionBoxCheck()
    {
        // -- Micro Suggestions Check --
        List<string> microSC = new List<string>();
        foreach (HackObject hack in MapManager.inst.hackDatabase.Hack)
        {
            if (hack.name.ToLower().Contains(HF.GetLeftSubstring(field.text))) // If this command is a variant of what the player is currently attempting to type
            {
                string newCom = HF.GetRightSubstring(HF.ParseHackName(hack));
                microSC.Add(newCom);
            }
        }

        microSC = microSC.Distinct().ToList(); // Remove duplicates
        // --

        if(microSC.Count > 7) // Too many suggestions, do the simplified version
        {
            ClearSuggestions();

        }
        else // Not that many suggestions, use the window
        {
            if (field.text.Contains("(") && field.text[field.text.Length - 1].ToString() != ")")
            {
                if (!filled)
                    FillSuggestions();
            }
            else
            {
                if (filled)
                    ClearSuggestions();
            }

            // Now for navigation
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
    }

    private void IndicateSelectedSuggestion(int id)
    {
        // Gray (dark green) them all out.
        foreach(GameObject item in activeSuggestions.ToList())
        {
            TextMeshProUGUI[] textUGUIs2 = item.GetComponentsInChildren<TextMeshProUGUI>();
            textUGUIs2[1].color = darkGreenColor; // We don't want the fill text
            item.GetComponentInChildren<TextMeshProUGUI>().color = darkGreenColor;
        }

        // Indicate the selected one
        TextMeshProUGUI[] textUGUIs = activeSuggestions[id].GetComponentsInChildren<TextMeshProUGUI>();
        textUGUIs[1].color = lowDetColor; // We don't want the fill text
    }

    private void FillSuggestions()
    {
        // Open the window
        box_main.SetActive(true);
        box_small.SetActive(true);

        // Set the positions correctly (we are basing it off of the top right corner of the field.textComponent
        Vector3[] v = new Vector3[4];
        backerText.GetComponent<RectTransform>().GetWorldCorners(v);
        float currentX = v[2].x;

        box_main.transform.position = new Vector3(currentX, box_main.transform.position.y, 0);
        box_small.transform.position = new Vector3(currentX, box_small.transform.position.y, 0);

        // Create list of suggestions
        suggestions.Clear();
        int floorRating = MapManager.inst.currentLevel + 11; // ex: -10 + 11 = 1
        foreach (HackObject hack in MapManager.inst.hackDatabase.Hack)
        {
            if (hack.name.ToLower().Contains(HF.GetLeftSubstring(field.text))) // If this command is a variant of what the player is currently attempting to type
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
                        if(bot.tier == rating && !bot.playerHasAnalysisData && bot.tier >= floorRating)
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
                    string newCom = HF.GetRightSubstring(HF.ParseHackName(hack));
                    suggestions.Add(newCom);
                }
            }
        }

        suggestions = suggestions.Distinct().ToList(); // Remove duplicates

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

        // Set current suggestion as first one
        suggestionID = 0;
        currentSuggestion = suggestions[suggestionID];
        box_suggestionText.text = currentSuggestion + ")";

        // Highlight the first suggestion in the list
        IndicateSelectedSuggestion(suggestionID);

        filled = true;
    }

    private void ClearSuggestions()
    {
        // Clear active suggestions
        foreach (GameObject item in activeSuggestions.ToList())
        {
            Destroy(item);
        }
        activeSuggestions.Clear();

        currentSuggestion = "";
        box_suggestionText.text = "";
        suggestionID = -1;

        // Close the window
        box_main.SetActive(false);
        box_small.SetActive(false);

        filled = false;
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
        StartCoroutine(ShutdownAnim());
    }

    private IEnumerator ShutdownAnim()
    {

        yield return null;

        Destroy(this.gameObject);

    }
}

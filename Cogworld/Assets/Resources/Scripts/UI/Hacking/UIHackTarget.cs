using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;

/// <summary>
/// Used for the the hacking targets in the hacking screen. "a - [Do a thing] ------------ ##%"
/// </summary>
public class UIHackTarget : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI letterText;
    public TextMeshProUGUI LB;
    public TextMeshProUGUI RB;
    public TextMeshProUGUI optionalText;
    public TextMeshProUGUI primaryText;
    public TextMeshProUGUI backerText; // The black highlight
    public TextMeshProUGUI lineText;
    public TextMeshProUGUI percentChanceText;

    [Header("Command")]
    public TerminalCommand command = null;

    [Header("Color Pallet")]
    public Color darkGreenColor;
    public Color grayedOutColor;
    public Color lowDetColor;
    public Color mediumDetColor;
    public Color highDetColor;
    public Color veryHighDetColor;

    // Info
    private string assignedLetter;
    [SerializeField] private float chanceOfSuccess;
    private bool available = true;
    private string setText;
    private string optText = "";
    public bool isManualCommand = false;

    [Header("Button Related")]
    public GameObject buttonStuff;
    public Image buttonBacker;


    public void Setup(TerminalCommand commandToUse, bool drawLine, float successChance, bool startAsUsed = false)
    {
        lineText.enabled = drawLine;
        chanceOfSuccess = successChance;
        command = commandToUse;
        assignedLetter = commandToUse.assignedChar;
        setText = commandToUse.command;
        optText = commandToUse.secondaryText;
        available = DetermineIfAvaiable(commandToUse);
        SetKey();

        // Set # Amount & Color (chance of success)
        if (chanceOfSuccess >= 0.8f) // V. High
        {
            percentChanceText.color = lowDetColor;
        }
        else if (chanceOfSuccess < 0.8f && chanceOfSuccess >= 0.6f) // High
        {
            percentChanceText.color = mediumDetColor;
        }
        else if (chanceOfSuccess < 0.6f && chanceOfSuccess >= 0.3f) // Medium
        {
            percentChanceText.color = highDetColor;
        }
        else // Low
        {
            percentChanceText.color = veryHighDetColor;
        }
        percentChanceText.text = (int)(chanceOfSuccess * 100) + "%";
        letterText.text = assignedLetter + "-";

        if (startAsUsed)
        {
            available = false;
        }
        AppearAnim();
    }

    public void SetupAsManualCommand(bool drawLine)
    {
        assignedLetter = "z";
        SetKey();
        lineText.enabled = drawLine;

        isManualCommand = true;

        optText = "";
        setText = "Manual Command";
        primaryText.text = setText;

        percentChanceText.text = "N/A";
        percentChanceText.color = darkGreenColor;

        letterText.text = assignedLetter + "-";

        AppearAnim();
    }

    KeyControl assignedKey;
    bool ready = false;
    private void SetKey()
    {
        assignedKey = Keyboard.current[assignedLetter.ToLower()] as KeyControl;
    }

    private void Update()
    {
        // Input listener
        if (available && ready && UIManager.inst.terminal_activeIField == null) // Don't want to accept input when doing so somewhere else!
        {
            if (assignedKey.wasPressedThisFrame && !GlobalSettings.inst.db_main.gameObject.activeInHierarchy) // Make sure to not trigger when typing in console
            {
                AttemptHack();
            }
        }
    }

    public void AttemptHack()
    {
        if (!isManualCommand)
        {
            float random = Random.Range(0.0f, 1.0f);

            string fill = "";
            if (command.item != null)
            {
                fill = command.item.itemName;
            }
            else if (command.bot != null)
            {
                fill = command.bot.botName;
            }
            else if (command.knowledge != null)
            {
                fill = command.knowledge.name;
            }

            // If its an open system we auto-succeed
            if (UIManager.inst.terminal_targetTerm.secLvl == 0)
            {
                SucceedHack(fill);
            }
            else
            {
                if (random <= chanceOfSuccess) // SUCCESS
                {
                    SucceedHack(fill);
                }
                else // FAILURE
                {
                    UIManager.inst.Terminal_CreateResult(HF.GenericHackFailure(random), highDetColor, (">>" + HF.HackToPrintout(command.hack, fill)), true, chanceOfSuccess - random);
                }
            }
        }
        else // Manual Command stuff here
        {
            if(UIManager.inst.terminal_activeIField  == null) // dont wanna double-up
            {
                // Play the "MANUAL" sound (56)
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.dict_ui["MANUAL"]); // UI - MANUAL
                UIManager.inst.Terminal_CreateManualInput();
            }
        }
        HF.ScrollToBottom(UIManager.inst.terminal_resultsScrollrect); // Force scroll to bottom
    }

    private void SucceedHack(string fill)
    {
        string header = ">>" + HF.HackToPrintout(command.hack, fill);
        string rewardString = HF.MachineReward_PrintPLUSAction(command, command.item, command.bot);

        if (rewardString.Length > 0)
        {
            SetAsUsed();
            // Create result in terminal
            UIManager.inst.Terminal_CreateResult(rewardString, lowDetColor, header, true);
            // Create log messages
            UIManager.inst.CreateNewLogMessage(header, lowDetColor, darkGreenColor, true);
            UIManager.inst.CreateNewLogMessage(rewardString, UIManager.inst.deepInfoBlue, UIManager.inst.infoBlue, true, true);
        }
    }

    public void AppearAnim()
    {
        if (isManualCommand)
        {
            this.gameObject.name = "Z - Manual Command";
        }
        else
        {
            this.gameObject.name = assignedKey + " - " + command.command;
        }

        if (available)
        {
            StartCoroutine(AnimateReveal());
        }
        else
        {
            string combinedText = optText + " " + setText;

            try
            {
                lineText.text = string.Concat(Enumerable.Repeat("-", (30 - combinedText.Length)));
            }
            catch (System.Exception)
            {
                lineText.text = "-------"; // Failsafe
            }

            // Set the backer text
            backerText.text = HF.GenerateMarkedString(combinedText); // Mark highlights it as pure black

            primaryText.text = setText;
            optionalText.text = optText;

            SetAsUsed();
        }
    }

    IEnumerator AnimateReveal()
    {
        float delay = 0.1f;
        // We need to animate in:
        // -The assigned letter
        // -The two brackets
        // -The primary text
        // -The dashed line (if needed)
        // -The percent success
        // -The second darker primary text (if needed)

        // All this stuff just flashes in

        string combinedText = optText + " " + setText;

        combinedText += "]";

        try
        {
            lineText.text = string.Concat(Enumerable.Repeat("-", (30 - combinedText.Length)));
        }
        catch (System.Exception)
        {
            lineText.text = "-------"; // Failsafe
        }
        

        // Set the backer text
        backerText.text = HF.GenerateMarkedString(combinedText); // Mark highlights it as pure black
        Color percentColor = percentChanceText.color;

        primaryText.text = setText;
        optionalText.text = optText;

        float value = 0f;

        letterText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        LB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        RB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        lineText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        optionalText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        primaryText.color = new Color(lowDetColor.r, lowDetColor.g, lowDetColor.b, value);
        percentChanceText.color = new Color(percentColor.r, percentColor.g, percentColor.b, value);

        yield return new WaitForSeconds(delay);

        value = 0.5f;
        letterText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        LB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        RB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        lineText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        optionalText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        primaryText.color = new Color(lowDetColor.r, lowDetColor.g, lowDetColor.b, value);
        percentChanceText.color = new Color(percentColor.r, percentColor.g, percentColor.b, value);

        yield return new WaitForSeconds(delay);

        value = 0.25f;
        letterText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        LB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        RB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        lineText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        optionalText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        primaryText.color = new Color(lowDetColor.r, lowDetColor.g, lowDetColor.b, value);
        percentChanceText.color = new Color(percentColor.r, percentColor.g, percentColor.b, value);

        yield return new WaitForSeconds(delay);

        value = 0.75f;
        letterText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        LB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        RB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        lineText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        optionalText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        primaryText.color = new Color(lowDetColor.r, lowDetColor.g, lowDetColor.b, value);
        percentChanceText.color = new Color(percentColor.r, percentColor.g, percentColor.b, value);

        yield return new WaitForSeconds(delay);

        value = 0.5f;
        letterText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        LB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        RB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        lineText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        optionalText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        primaryText.color = new Color(lowDetColor.r, lowDetColor.g, lowDetColor.b, value);
        percentChanceText.color = new Color(percentColor.r, percentColor.g, percentColor.b, value);

        yield return new WaitForSeconds(delay);

        value = 1f;
        letterText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        LB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        RB.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        lineText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        optionalText.color = new Color(darkGreenColor.r, darkGreenColor.g, darkGreenColor.b, value);
        primaryText.color = new Color(lowDetColor.r, lowDetColor.g, lowDetColor.b, value);
        percentChanceText.color = new Color(percentColor.r, percentColor.g, percentColor.b, value);

        yield return null;

        ready = true;
    }

    public void SetAsUsed()
    {
        available = false;
        if(!command.repeatable) 
            command.available = false; // Set command as being used

        letterText.gameObject.SetActive(false); // Disable: # -
        LB.gameObject.SetActive(false);         // Disable: [
        RB.gameObject.SetActive(false);         // Disable: ]

        percentChanceText.text = "N/A";
        percentChanceText.color = darkGreenColor;

        optionalText.color = grayedOutColor;
        primaryText.color = grayedOutColor;

        buttonBacker.enabled = false;
        buttonStuff.GetComponent<SetColor>().enabled = false;
        buttonStuff.GetComponent<Button>().enabled = false;
        buttonStuff.GetComponent<UIHoverEvent>().enabled = false;

        StartCoroutine(UsedAnim());
    }

    private IEnumerator UsedAnim()
    {
        // Just flash the optional/primary text a bit
        float delay = 0.1f;

        optionalText.gameObject.SetActive(true);
        primaryText.gameObject.SetActive(true);

        optionalText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);
        primaryText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        optionalText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);
        primaryText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);

        yield return new WaitForSeconds(delay);

        optionalText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.75f);
        primaryText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.75f);

        yield return new WaitForSeconds(delay);

        optionalText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);
        primaryText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        optionalText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);
        primaryText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);

        yield return new WaitForSeconds(delay);

        optionalText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 1f);
        primaryText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 1f);
    }

    /// <summary>
    /// Force this target to be non-interactable.
    /// </summary>
    public void ForceDisabled()
    {
        this.GetComponent<Button>().enabled = false;
        this.GetComponent<UIHoverEvent>().disabled = true;
        this.GetComponent<UIHoverEvent>().enabled = false;
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

    private bool DetermineIfAvaiable(TerminalCommand command)
    {
        // Usually its true, but will sometimes be false if:
        // - The command is for a record the player already has
        // - The command is for a schematic / analysis the player already has
        // - Is of type NONE (Like "No Schematic Loaded")

        if(command.subType == TerminalCommandType.NONE) // These are unavailable by default
        {
            return false;
        }
        else if (command.subType == TerminalCommandType.Schematic)
        {
            if (command.bot)
            {
                return !command.bot.schematicDetails.hasSchematic;
            }
            else if (command.item)
            {
                return !command.item.knowByPlayer;
            }
        }
        else if (command.subType == TerminalCommandType.Analysis)
        {
            if (command.bot)
            {
                return !command.bot.playerHasAnalysisData;
            }
        }
        else if (command.subType == TerminalCommandType.Query)
        {
            return !command.knowledge.knowByPlayer;
        }


        return true;
    }

    public void PlayHoverSound()
    {
        // Play the hover UI sound
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["HOVER"]); // UI - HOVER
    }

    public void MouseHoverUpdate(bool mouseEnter)
    {
        UIManager.inst.Terminal_UpdateHoveredChoice(this.gameObject, mouseEnter);
    }
}

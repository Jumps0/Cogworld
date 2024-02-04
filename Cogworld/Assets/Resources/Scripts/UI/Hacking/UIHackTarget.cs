using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

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
    private bool isManualCommand = false;

    [Header("Button Related")]
    public GameObject buttonStuff;
    public Image buttonBacker;


    public void Setup(TerminalCommand commandToUse, bool drawLine, float successChance)
    {
        lineText.enabled = drawLine;
        chanceOfSuccess = successChance;
        command = commandToUse;
        assignedLetter = commandToUse.assignedChar;
        setText = commandToUse.command;
        optText = commandToUse.secondaryText;
        available = DetermineIfAvaiable();
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

    KeyCode assignedKey;
    bool ready = false;
    private void SetKey()
    {
        assignedKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), assignedLetter.ToUpper());
    }

    private void Update()
    {
        // Input listener
        if (available && ready && UIManager.inst.terminal_activeInput == null) // Don't want to accept input when doing so somewhere else!
        {
            if (Input.GetKeyDown(assignedKey))
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

            if (random <= chanceOfSuccess) // SUCCESS
            {
                string fill = "";
                if(command.hack.hackType == TerminalCommandType.Query || command.hack.hackType == TerminalCommandType.Analysis || command.hack.hackType == TerminalCommandType.Schematic)
                {
                    fill = setText;
                }

                string header = ">>" + HF.ParseHackName(command.hack, fill);
                string rewardString = HF.MachineReward_PrintPLUSAction(command);
                SetAsUsed();
                // Create result in terminal
                UIManager.inst.Terminal_CreateResult(rewardString, lowDetColor, header);
                // Create log messages
                UIManager.inst.CreateNewLogMessage(header, lowDetColor, darkGreenColor, true);
                UIManager.inst.CreateNewLogMessage(rewardString, UIManager.inst.deepInfoBlue, UIManager.inst.infoBlue, true, true);
            }
            else // FAILURE
            {
                UIManager.inst.Terminal_CreateResult(HF.GenericHackFailure(random), highDetColor, (">>" + HF.ParseHackName(command.hack)), true);
            }
        }
        else // Manual Command stuff here
        {
            if(UIManager.inst.terminal_activeInput  == null) // dont wanna double-up
            {
                // Play the "MANUAL" sound (51)
                AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[51]);
                UIManager.inst.Terminal_CreateManualInput();
            }
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
        StartCoroutine(AnimateReveal());
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
        backerText.text = "<mark=#000000>" + combinedText + "</mark>"; // Mark highlights it as pure black
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
        command.available = false;

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

    public void ShutDown()
    {
        StartCoroutine(ShutdownAnim());
    }

    private IEnumerator ShutdownAnim()
    {

        yield return null;

        Destroy(this.gameObject);

    }

    private bool DetermineIfAvaiable()
    {
        // This needs to a bunch of research stuff
        // Usually its true, but will sometimes be false if:
        // - The command is for a record the player already has
        // - The command is for a schematic / analysis the player already has

        return true;
    }
}

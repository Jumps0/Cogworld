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
/// Support script for an individual custom hacking code.
/// </summary>
public class UIHackCustomCode : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI assignedLetterText;
    public TextMeshProUGUI codeText;
    public TextMeshProUGUI sourceText;
    public TextMeshProUGUI targetText;


    [Header("Command")]
    public TerminalCustomCode code = null;

    [Header("Color Pallet")]
    public Color fadedGray;
    public Color headerWhite;
    public Color darkGreenColor;
    public Color grayedOutColor;
    public Color lowDetColor;
    public Color mediumDetColor;
    public Color highDetColor;
    public Color veryHighDetColor;

    [Header("Button Related")]
    public GameObject buttonStuff;
    public Image buttonBacker;
    public string assignedKey = "";

    public void Setup(TerminalCustomCode codeToUse, string key)
    {
        code = codeToUse;
        assignedKey = key;
        SetKey();

        // Set the text
        assignedLetterText.text = (assignedKey + "-");
        codeText.text = "\\\\" + code.code;
        sourceText.text = code.source;
        targetText.text = code.target;

        AppearAnim();

        available = true;
    }

    KeyControl assignedKeyCode;
    bool ready = false;
    bool available = false;

    private void SetKey()
    {
        assignedKeyCode = Keyboard.current[assignedKey.ToLower()] as KeyControl;
    }

    private void Update()
    {
        // Input listener
        if (available && ready && UIManager.inst.terminal_activeIField == null) // Don't want to accept input when doing so somewhere else!
        {
            if (assignedKeyCode.wasPressedThisFrame && !GlobalSettings.inst.db_main.gameObject.activeInHierarchy) // Make sure to not trigger when typing in console
            {
                AttemptHack();
            }
        }
    }

    public void AttemptHack()
    {
        WorldTile tile = MapManager.inst.mapdata[UIManager.inst.terminal_targetTerm.x, UIManager.inst.terminal_targetTerm.y];

        if (tile.machinedata.type == MachineType.Terminal && tile.machinedata.terminalCustomCodes.Count > 0)
        {
            // Does the connected terminal accept this custom code?
            if (tile.machinedata.terminalCustomCodes.Contains(code))
            {
                // Activate the code!
                tile.machinedata.UseCustomCode(code);
                SetAsUsed();
            }
            else // Do nothing (dummy)
            {
                SetAsUsed();
                UIManager.inst.Terminal_CreateResult("Unknown command.", highDetColor, codeText.text, true, 0.5f);
            }
        }
        
    }

    public void AppearAnim()
    {
        StartCoroutine(AnimateReveal());
    }

    IEnumerator AnimateReveal()
    {
        float delay = 0.1f;

        Color usedColor = lowDetColor;

        assignedLetterText.color = new Color(headerWhite.r, headerWhite.g, headerWhite.b, 0f);
        codeText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        sourceText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);
        targetText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);

        yield return new WaitForSeconds(delay);

        assignedLetterText.color = new Color(headerWhite.r, headerWhite.g, headerWhite.b, 0.2f);
        codeText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        sourceText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);
        targetText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);

        yield return new WaitForSeconds(delay);

        assignedLetterText.color = new Color(headerWhite.r, headerWhite.g, headerWhite.b, 0.4f);
        codeText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        sourceText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);
        targetText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);

        yield return new WaitForSeconds(delay);

        assignedLetterText.color = new Color(headerWhite.r, headerWhite.g, headerWhite.b, 0.6f);
        codeText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        sourceText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);
        targetText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);

        yield return new WaitForSeconds(delay);

        assignedLetterText.color = new Color(headerWhite.r, headerWhite.g, headerWhite.b, 0.8f);
        codeText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.8f);
        sourceText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.8f);
        targetText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.8f);

        yield return new WaitForSeconds(delay);

        assignedLetterText.color = new Color(headerWhite.r, headerWhite.g, headerWhite.b, 1f);
        codeText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        sourceText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);
        targetText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);

        yield return null;

        ready = true;
    }

    public void SetAsUsed()
    {
        
        available = false;

        assignedLetterText.text = "--";

        codeText.color = grayedOutColor;
        sourceText.color = grayedOutColor;
        targetText.color = grayedOutColor;

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


        codeText.gameObject.SetActive(true);
        sourceText.gameObject.SetActive(true);
        targetText.gameObject.SetActive(true);

        codeText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);
        sourceText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);
        targetText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        codeText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);
        sourceText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);
        targetText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);

        yield return new WaitForSeconds(delay);

        codeText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.75f);
        sourceText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.75f);
        targetText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.75f);

        yield return new WaitForSeconds(delay);

        codeText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);
        sourceText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);
        targetText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.25f);

        yield return new WaitForSeconds(delay);

        codeText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);
        sourceText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);
        targetText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 0.5f);

        yield return new WaitForSeconds(delay);

        codeText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 1f);
        sourceText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 1f);
        targetText.color = new Color(grayedOutColor.r, grayedOutColor.g, grayedOutColor.b, 1f);


        yield return null;
    }

    public void ShutDown()
    {
        Destroy(this.gameObject);
    }
}

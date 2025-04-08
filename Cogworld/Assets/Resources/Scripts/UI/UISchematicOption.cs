using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISchematicOption : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI text_letter;
    public TextMeshProUGUI text_main;
    public Image image_backer;
    public Image image_buttonindicator;

    [Header("Assignment")]
    [HideInInspector]public ItemObject item = null;
    [HideInInspector] public BotObject bot = null;
    public string letter;
    [SerializeField] private float chanceOfSuccess;
    private TerminalCommand command;

    [Header("Colors")]
    public Color darkGreenColor;
    public Color lowDetColor;
    public Color highDetColor;
    public Color colorGreen;

    public void Init(string _letter, float delay, ItemObject _item = null, BotObject _bot = null)
    {
        letter = _letter;
        text_letter.text = letter + "-";

        if(_item != null )
        {
            text_main.text = HF.RecolorPrefix(_item, "034000");
        }
        else if(_bot != null )
        {
            text_main.text = _bot.botName;
        }

        this.GetComponent<RectTransform>().sizeDelta = new Vector2(350, this.GetComponent<RectTransform>().sizeDelta.y); // Un-stretch the rectTransform

        // Calculate chance of success
        item = _item;
        bot = _bot;
        command = new TerminalCommand(letter, "Load Schematic", TerminalCommandType.Load, "", MapManager.inst.hackDatabase.Hack[26], null, null, _bot, _item);

        int secLvl = MapManager.inst.mapdata[UIManager.inst.terminal_targetTerm.x, UIManager.inst.terminal_targetTerm.y].machinedata.secLvl;
        float chance = 0f;
        if (secLvl == 0)
        {
            chance = 1f; // Open System
        }
        else
        {
            HackObject hack = command.hack;
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
        chanceOfSuccess = chance;

        StartCoroutine(AnimateAppear(delay));
    }

    private IEnumerator AnimateAppear(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float elapsedTime = 0f;
        float duration = 0.35f;
        while (elapsedTime < duration) // Black -> Green
        {
            image_backer.color = Color.Lerp(Color.black, colorGreen, elapsedTime / duration);
            image_buttonindicator.color = Color.Lerp(Color.black, colorGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        image_backer.color = colorGreen;
        image_buttonindicator.color = colorGreen;
        duration = 0.35f;
        while (elapsedTime < duration) // Green -> Black
        {
            image_backer.color = Color.Lerp(colorGreen, Color.black, elapsedTime / duration);
            image_buttonindicator.color = Color.Lerp(colorGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        image_backer.color = Color.black;
        image_buttonindicator.color = Color.black;


        #region Fancy Method
        /*
        // Get the text from TextMeshPro
        string text = text_main.text;

        // Store the original text without any markup
        string originalText = text_main.GetParsedText();

        // Initialize a list to track the markup state of each character
        List<bool> isMarked = new List<bool>();
        for (int i = 0; i < text.Length; i++)
        {
            // Assume all characters are marked initially
            isMarked.Add(true);
        }

        // Randomly unmark characters over time
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            // Choose a random character index to unmark
            int index = Random.Range(0, text.Length);

            // Unmark the character
            isMarked[index] = false;

            // Construct the text with current markup state
            string markedText = GetMarkedText(originalText, isMarked);

            // Update the TextMeshPro text
            text_main.text = markedText;

            // Wait for a short interval
            yield return new WaitForSeconds(0.01f);

            // Increment elapsed time
            elapsedTime += 0.01f;
        }

        // Ensure all characters are unmarked at the end
        for (int i = 0; i < isMarked.Count; i++)
        {
            isMarked[i] = false;
        }

        // Construct the final text with no markup
        string finalText = GetMarkedText(originalText, isMarked);

        // Update the TextMeshPro text to the final text
        text_main.text = finalText;
        */
        #endregion
    }

    string GetMarkedText(string originalText, List<bool> isMarked)
    {
        // Create a StringBuilder to construct the marked text
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Iterate through each character and add or remove markup based on the markup state
        for (int i = 0; i < originalText.Length; i++)
        {
            if (isMarked[i])
            {
                sb.Append($"<mark=#{ColorUtility.ToHtmlStringRGBA(lowDetColor)}>{originalText[i]}</mark>");
            }
            else
            {
                sb.Append(originalText[i]);
            }
        }

        return sb.ToString();
    }

    public void AttemptLoad()
    {
        float random = Random.Range(0.0f, 1.0f);

        string fill = "";
        if (item != null)
        {
            fill = item.itemName;
        }
        else if (bot != null)
        {
            fill = bot.botName;
        }

        // If its an open system we auto-succeed
        if (MapManager.inst.mapdata[UIManager.inst.terminal_targetTerm.x, UIManager.inst.terminal_targetTerm.y].machinedata.secLvl == 0)
        {
            SucceedHack(fill);
        }
        else
        {
            if (random <= chanceOfSuccess)
            { // SUCCESS
                SucceedHack(fill);
            }
            else // FAILURE
            {
                UIManager.inst.Terminal_CreateResult(HF.GenericHackFailure(random), highDetColor, (">>" + HF.HackToPrintout(command.hack)), true, chanceOfSuccess - random);
            }
            HF.ScrollToBottom(UIManager.inst.terminal_resultsScrollrect); // Force scroll to bottom
        }
    }

    private void SucceedHack(string fill)
    {
        string rewardString = HF.MachineReward_PrintPLUSAction(command, item, bot);
        string header = ">>" + HF.HackToPrintout(command.hack, fill);

        // Create result in terminal
        UIManager.inst.Terminal_CreateResult(rewardString, lowDetColor, header, true);
        // Create log messages
        UIManager.inst.CreateNewLogMessage(header, lowDetColor, darkGreenColor, true);
        UIManager.inst.CreateNewLogMessage(rewardString, UIManager.inst.deepInfoBlue, UIManager.inst.infoBlue, true, true);
    }
}

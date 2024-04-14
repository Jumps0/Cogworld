using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.VersionControl;

public class UIDataGenericDetail : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI primary_text;
    // Secondary
    public GameObject secondaryParent;
    public TextMeshProUGUI valueA_text;
    public TextMeshProUGUI secondary_text;
    private bool secondary_fadedOut;
    private bool valueA_fadedOut;
    // - Variable Box
    public GameObject variableBox;
    public Image variableB_image;
    public TextMeshProUGUI variableB_text;
    private Color variable_color;
    // - Box Bar
    public GameObject boxBarParent;
    public List<GameObject> mainBoxes;
    public List<GameObject> secondaryBoxes;
    private float barAmount;

    [Header("Colors")] // For the boxes. Other stuff uses colors from UIManager
    public Color b_green;
    public Color b_yellow;
    public Color b_red;
    //
    public Color highlightColor;
    public Color brightGreen;
    public Color darkGreen;

    public void Setup(bool useSecondary, bool useVariable, bool useBoxBar, string mainText, Color boxColor, string valueA = "", bool valueA_faded = false, string secondaryText = "", bool secondary_faded = false, string boxText = "", float _barAmount = 0f)
    {
        primary_text.text = mainText;

        // Secondary
        ToggleSecondaryState(useSecondary);
        if (useSecondary && secondaryText != "")
        {
            secondary_text.gameObject.SetActive(true);

            secondary_text.text = secondaryText;
            if(secondary_faded)
            {
                secondary_text.color = darkGreen;
            }
            secondary_fadedOut = secondary_faded;
        }
        else
        {
            secondary_text.gameObject.SetActive(false);
        }

        if(valueA != "")
        {
            valueA_text.text = valueA;
            if (valueA_faded)
            {
                valueA_text.color = darkGreen;
            }
            valueA_fadedOut = valueA_faded;
        }
        else
        {
            valueA_text.gameObject.SetActive(false);
        }

        // Variable Box
        ToggleVariableBox(useVariable);
        if (useVariable)
        {
            variable_color = boxColor;
            SetVariableBox(boxText, boxColor);
        }

        // Box Bar
        ToggleBoxBar(useBoxBar);
        if (useBoxBar)
        {
            barAmount = _barAmount;
            UpdateBoxIndicator(0f); // Starts out at 0 and animates out
        }
    }

    private void ToggleSecondaryState(bool active)
    {
        secondaryParent.SetActive(active);
    }

    private void ToggleVariableBox(bool active)
    {
        variableBox.SetActive(active);
    }

    public void SetVariableBox(string text, Color color)
    {
        variableB_image.color = color;
        variableB_text.text = text;
    }

    #region Box Bar
    private void ToggleBoxBar(bool active)
    {
        boxBarParent.SetActive(active);
    }

    // Update the indicator bar based on the value (0-100%)
    public void UpdateBoxIndicator(float value)
    {
        int activeBoxes = Mathf.RoundToInt(mainBoxes.Count * value / 100f);

        for (int i = 0; i < mainBoxes.Count; i++)
        {
            if (i < activeBoxes)
            {
                // Activate main box
                //mainBoxes[i].SetActive(true);
                mainBoxes[i].GetComponent<Image>().color = DetermineColor(value);

                // Deactivate secondary box
                secondaryBoxes[i].SetActive(false);
            }
            else
            {
                // Activate secondary box
                secondaryBoxes[i].SetActive(true);
                secondaryBoxes[i].GetComponent<Image>().color = Color.black;

                // Deactivate main box
                //mainBoxes[i].SetActive(false);
            }
        }
    }

    // Determine the color of the main box based on the value
    private Color DetermineColor(float value)
    {
        if (value >= 66f)
        {
            return b_green;
        }
        else if (value <= 33f)
        {
            return b_red;
        }
        else
        {
            return b_yellow;
        }
    }
    #endregion

    #region Animation
    public void Open()
    {
        StartCoroutine(AnimateOpen());
    }

    private IEnumerator AnimateOpen()
    {
        // Trigger the other animations if needed
        if (secondary_text.gameObject.activeInHierarchy && !secondary_fadedOut) // Activate the secondary text animation if needed
        {
            StartCoroutine(OpenSecondary());
        }
        if(valueA_text.gameObject.activeInHierarchy && !valueA_fadedOut)
        {
            StartCoroutine(OpenValueA());
        }
        if (variableBox.gameObject.activeInHierarchy)
        {
            StartCoroutine(OpenVariableBox());
        }
        if (boxBarParent.gameObject.activeInHierarchy)
        {
            StartCoroutine(OpenBoxBar());
        }

        string primaryStart = primary_text.text;
        int len = primaryStart.Length;

        float delay = 0f;
        float perDelay = 0.75f / len;
        primary_text.text = "";

        List<string> segments = HF.StringToList(primaryStart);

        foreach (string segment in segments)
        {
            string s = segment;
            string last = HF.GetLastCharOfString(s);
            if(last == " ")
            {
                last = "_"; // Janky workaround because mark doesn't highlight spaces
            }

            if (s.Length > 0)
            {
                s = segment.Remove(segment.Length - 1, 1); // Remove the last character
                if(last == "_")
                {
                    s += $"<mark=#{ColorUtility.ToHtmlStringRGB(highlightColor)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{last}</color></mark>"; // Add it back with the highlight
                }
                else
                {
                    s += $"<mark=#{ColorUtility.ToHtmlStringRGB(highlightColor)}aa><color=#{ColorUtility.ToHtmlStringRGB(brightGreen)}>{last}</color></mark>"; // Add it back with the highlight
                }
            }

            StartCoroutine(HF.DelayedSetText(primary_text, s, delay += perDelay));
        }

        yield return new WaitForSeconds(delay);

        primary_text.text = primaryStart;
    }

    private IEnumerator OpenSecondary()
    {
        string secondaryStart = secondary_text.text;
        int len = secondaryStart.Length;

        float delay = 0f;
        float perDelay = 0.75f / len;
        secondary_text.text = "";

        List<string> segments = HF.StringToList(secondaryStart);

        foreach (string segment in segments)
        {
            string s = segment;
            string last = HF.GetLastCharOfString(s);
            if (last == " ")
            {
                last = "_"; // Janky workaround because mark doesn't highlight spaces
            }

            if (s.Length > 0)
            {
                s = segment.Remove(segment.Length - 1, 1); // Remove the last character
                if (last == "_")
                {
                    s += $"<mark=#{ColorUtility.ToHtmlStringRGB(highlightColor)}aa><color=#{ColorUtility.ToHtmlStringRGB(Color.black)}>{last}</color></mark>"; // Add it back with the highlight
                }
                else
                {
                    s += $"<mark=#{ColorUtility.ToHtmlStringRGB(highlightColor)}aa><color=#{ColorUtility.ToHtmlStringRGB(brightGreen)}>{last}</color></mark>"; // Add it back with the highlight
                }
            }

            StartCoroutine(DelayedSetText(secondary_text, s, delay += perDelay));
        }

        yield return new WaitForSeconds(delay);

        secondary_text.text = secondaryStart;
    }

    private IEnumerator OpenValueA()
    {
        // Instead of typing out like normal, this flashes from black -> bright green -> black like 3 times, then goes from black -> green.

        yield return null;
    }

    private IEnumerator OpenVariableBox()
    {
        // In summary this animation does the following:
        // 1. Black -> Bright [Color]
        // 2. Bright [Color] -> Black
        // 3. Black -> Bright [Color]
        // 4. Bright [Color] -> Black
        // 5. Black -> [Color]

        // While 5 is happening, the text gets a left -> right highlight pass.

        Color primaryColor = variable_color; // This is the color we will come back to
        Color brightColor = variable_color;
        if(brightColor.g + 0.1f < 1f) // This might work?
        {
            brightColor.g += 0.1f;
        }

        // 1
        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration) // Black -> Bright [Color]
        {
            variableB_image.color = Color.Lerp(Color.black, brightColor, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        variableB_image.color = brightColor;
        // 2
        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Bright [Color] -> Black
        {
            variableB_image.color = Color.Lerp(brightColor, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        variableB_image.color = Color.black;
        // 3
        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Black -> Bright [Color]
        {
            variableB_image.color = Color.Lerp(Color.black, brightColor, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        variableB_image.color = brightColor;
        // 4
        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Bright [Color] -> Black
        {
            variableB_image.color = Color.Lerp(brightColor, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        variableB_image.color = Color.black;
        // 5
        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Black -> [Color]
        {
            variableB_image.color = Color.Lerp(Color.black, primaryColor, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        variableB_image.color = primaryColor;

        // Now do the highlight thing
        string text = variableB_text.text;
        float delay = 0f;
        float perDelay = 0.25f / text.Length;
        for (int i = 0; i <= text.Length; i++)
        {
            // Construct the partially revealed text
            string revealedText = "";

            for (int j = 0; j < text.Length; j++)
            {
                // Highlight the current character if it's at index "i", otherwise include as is
                if (j == i)
                {
                    revealedText += $"<mark=#{ColorUtility.ToHtmlStringRGB(variableB_text.color)}aa>{text[j]}</mark>";
                }
                else
                {
                    revealedText += text[j];
                }
            }

            StartCoroutine(HF.DelayedSetText(variableB_text, revealedText, delay += perDelay));
        }

        yield return new WaitForSeconds(delay);

        variableB_text.text = text;
    }

    private IEnumerator OpenBoxBar()
    {
        // The box is starting out completely empty.
        // We need to "expand outward" towards the true value.
        // How this works is:
        // 1. We set a box we know will be filled to be completely black.
        // 2. We fade this box from black to its true state/color.
        // 3. Halfway through this process ^, we start with the next box, until we reach our true value.

        yield return null;
    }

    private IEnumerator DelayedSetText(TextMeshProUGUI UI, string text, float delay)
    {
        yield return new WaitForSeconds(delay);

        UI.text = text;
    }

    public void Close()
    {
        StartCoroutine(AnimateClose());
    }

    private IEnumerator AnimateClose()
    {
        yield return null;

        Destroy(this.gameObject);
    }
    #endregion
}

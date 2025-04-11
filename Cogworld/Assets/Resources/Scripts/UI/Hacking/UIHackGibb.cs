using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Used for custom terminals, generates the 8 lines of gibberish with 4 random highlights.
/// </summary>
public class UIHackGibb : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public string _message;

    [Header("Color Pallet")]
    public Color fadedGray;
    public Color headerWhite;
    public Color darkGreenColor;
    public Color grayedOutColor;
    public Color lowDetColor;
    public Color mediumDetColor;
    public Color highDetColor;
    public Color veryHighDetColor;

    [SerializeField] private float textSpeed = 0.05f;

    private void Start()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        // We need to generate 8, 46 character long strings of gibberish that each collapse down with their own little highlight

        List<string> strings = new List<string>();
        for (int i = 0; i < 8; i++)
        {
            strings.Add(GenerateRandomString(42)); // trying 42, to fit
        }

        // - Theres probably a better way of doing this but its only 8 lines so its not that bad.

        _text.text = "<mark=#04B101>" + strings[0] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += "<mark=#04B101>" + strings[1] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += strings[1];
        _text.text += "<mark=#04B101>" + strings[2] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += strings[1];
        _text.text += strings[2];
        _text.text += "<mark=#04B101>" + strings[3] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += strings[1];
        _text.text += strings[2];
        _text.text += strings[3];
        _text.text += "<mark=#04B101>" + strings[4] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += strings[1];
        _text.text += strings[2];
        _text.text += strings[3];
        _text.text += strings[4];
        _text.text += "<mark=#04B101>" + strings[5] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += strings[1];
        _text.text += strings[2];
        _text.text += strings[3];
        _text.text += strings[4];
        _text.text += strings[5];
        _text.text += "<mark=#04B101>" + strings[6] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += strings[1];
        _text.text += strings[2];
        _text.text += strings[3];
        _text.text += strings[4];
        _text.text += strings[5];
        _text.text += "<mark=#04B101>" + strings[6] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += strings[1];
        _text.text += strings[2];
        _text.text += strings[3];
        _text.text += strings[4];
        _text.text += strings[5];
        _text.text += strings[6];
        _text.text += "<mark=#04B101>" + strings[7] + "</mark>";

        yield return new WaitForSeconds(textSpeed);

        _text.text = strings[0];
        _text.text += strings[1];
        _text.text += strings[2];
        _text.text += strings[3];
        _text.text += strings[4];
        _text.text += strings[5];
        _text.text += strings[6];
        _text.text += strings[7];

        yield return new WaitForEndOfFrame();
        // -- And now --
        // Once the last string has generated we need to pick 4,
        // 12 character long areas to highlight, where the first & last character stay highlighted (the other is set to black text).

        HighlightRandomSnippets(_text.text);
        //AnimateTextHighlight();
    }

    private const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private string GenerateRandomString(int length)
    {
        string randomString = "";

        for (int i = 0; i < length; i++)
        {
            int randomIndex = Random.Range(0, characters.Length);
            randomString += characters[randomIndex];
        }

        return randomString;
    }

    private System.Random random = new System.Random();


    public void HighlightRandomSnippets(string inputString)
    {
        int stringLength = inputString.Length;
        int snippetLength = 12;
        int snippetCount = 4;
        int minSpacing = 20;

        if (stringLength < snippetLength * snippetCount + minSpacing * (snippetCount - 1))
        {
            Debug.LogError("Input string is not long enough to generate evenly spaced snippets.");
            return;
        }

        int totalSpaceRequired = snippetLength * snippetCount + minSpacing * (snippetCount - 1);
        float spaceBetweenSnippets = (stringLength - totalSpaceRequired) / (float)(snippetCount - 1);

        for (int i = 0; i < snippetCount; i++)
        {
            int startIndex = Mathf.RoundToInt((i + 1) * (snippetLength + spaceBetweenSnippets));

            string markedSnippet = "<mark=#04B101>" + inputString.Substring(startIndex, snippetLength) + "</mark>";
            inputString = inputString.Insert(startIndex, markedSnippet);
        }

        _text.text = inputString;

        LayoutRebuilder.ForceRebuildLayoutImmediate(UIManager.inst.terminalMenu.hackInfoArea.GetComponent<RectTransform>());
    }

    /*
    public void HighlightRandomSnippets(string inputString)
    {
        int stringLength = inputString.Length;
        int snippetLength = 12;
        string highlightColor = ColorUtility.ToHtmlStringRGB(lowDetColor);
        string centerColor = ColorUtility.ToHtmlStringRGB(lowDetColor);
        string targetColor = ColorUtility.ToHtmlStringRGB(Color.black);

        if (stringLength < snippetLength * 4)
        {
            Debug.LogError("Input string is not long enough to generate four 12-character snippets.");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            int startIndex = random.Next(0, stringLength - snippetLength);

            string markedSnippet = "<mark=#" + highlightColor + ">" + inputString.Substring(startIndex, snippetLength) + "</mark>";
            inputString = inputString.Insert(startIndex, markedSnippet);
            
            int centerIndex = startIndex + 1; // Index of the first character in the center (ignoring first and last)

            string colorFade = "<color=#" + centerColor + ">";
            string colorReset = "<color=#" + targetColor + ">";

            string animatedSnippet = inputString.Substring(centerIndex, snippetLength - 2); // Extract center characters
            animatedSnippet = colorFade + animatedSnippet + colorReset; // Wrap center characters with color tags

            inputString = inputString.Remove(centerIndex, snippetLength - 2); // Remove old center characters
            inputString = inputString.Insert(centerIndex, animatedSnippet); // Insert animated center characters
            

        }

        _text.text = inputString;
    }
    */

    private void AnimateTextHighlight()
    {
        TMP_TextInfo textInfo = _text.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (textInfo.characterInfo[i].character == '<')
            {
                int closingTagIndex = _text.text.IndexOf('>', i);
                int endIndex = closingTagIndex + 1;

                StartCoroutine(AnimateCharacterColor(i + 1, endIndex - 1, endIndex));
            }
        }
    }

    float animationDuration = 0.5f;

    private IEnumerator AnimateCharacterColor(int startIndex, int centerIndex, int endIndex)
    {
        Color targetColor = Color.black;
        Color initialColor = _text.color;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            while (elapsedTime < animationDuration)
            {
                float t = elapsedTime / animationDuration;
                Color lerpedColor = Color.Lerp(initialColor, targetColor, t);

                TMP_CharacterInfo[] characters = _text.textInfo.characterInfo;

                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (i >= centerIndex)
                    {
                        int charIndex = characters[i].index;
                        _text.textInfo.characterInfo[charIndex].color = lerpedColor;
                    }
                }

                _text.ForceMeshUpdate(true);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using ColorUtility = UnityEngine.ColorUtility;

public class UISmallQuest : MonoBehaviour
{
    [Header("Info")]
    public Quest quest;
    public bool selected = false;

    [Header("References")]
    [SerializeField] private Animator animator;

    // Header
    [SerializeField] private TextMeshProUGUI text_header;
    [SerializeField] private Image image_header_backer;
    private string header_name;
    //
    [SerializeField] private List<Image> image_border_bars;
    // Progress Bar
    [SerializeField] private Image image_bar_side;
    [SerializeField] private Image image_bar_main;
    [SerializeField] private Image image_bar_background;
    [Tooltip("The ##% percent that appears INSIDE the bar, indicating the % progress.")]
    [SerializeField] private TextMeshProUGUI text_bar;
    //
    [Tooltip("The ##/## that appears ABOVE the bar, indicating the amount progress.")]
    [SerializeField] private TextMeshProUGUI text_amount;
    [Tooltip("A shortened description of what you need to do for this quest.")]
    [SerializeField] private TextMeshProUGUI text_description;
    // Image
    [SerializeField] private Image image_main;
    [SerializeField] private Image image_main_borders;

    [Header("Colors")]
    public Color color_main = Color.white;
    public Color color_bright = Color.white;
    public Color color_dark = Color.white;

    public void Init(Quest quest, List<Color> colors, bool startAsSelected = false)
    {
        this.quest = quest;
        QuestObject info = quest.info;
        image_main.sprite = info.sprite;
        text_description.text = info.shortDescription;
        header_name = $"[{info.displayName}]";
        text_header.text = header_name;

        color_main = colors[0];
        color_bright = colors[1];
        color_dark = colors[2];

        // Then assign the set colors
        image_bar_side.color = color_main;
        image_bar_main.color = color_main;
        image_bar_background.color = color_dark;

        // Set the progress bar
        SetProgressBar();

        if(startAsSelected )
        {
            selected = true;
        }

        // Do the opening animation
        StartCoroutine(OpenAnimation());
    }

    public void SetProgressBar()
    {
        // We need to parse the quest steps into a number. Sometimes this may just be one, but sometimes its an actual number.

        QuestObject info = quest.info;

        int max = info.steps[quest.currentQuestStepIndex].amount;
        float current = quest.value;

        /*
        switch (info.type)
        {
            case QuestType.Default:
                Debug.LogWarning($"{info.name} has no set actions to do. It will not display properly!");
                break;
            case QuestType.Kill: // This will usually provide a straight number

                break;
            case QuestType.Collect: // May just be one, usually a list of something

                break;
            case QuestType.Find: // Variable

                break;
            case QuestType.Meet: // Usually just 1

                break;
            case QuestType.Destroy: // Variable

                break;
        }
        */

        // Set the text
        text_amount.text = $"{current}/{max}";

        // Set the percent
        float percent = (current / max);
        image_bar_main.fillAmount = percent;
        text_bar.text = $"{Mathf.RoundToInt(percent*100)}%";
        // We also need to make sure the % text is lined up next to the end of the progress bar.
        // (This is done in the opening animation)

    }

    public void Select()
    {
        if(!animating)
        {
            selected = true;
            text_header.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color_main)}>{header_name}</color>";
            QuestManager.inst.SelectQuest(this);
        }
    }

    public void Unselect()
    {
        if (!animating)
        {
            text_header.text = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>{header_name}</color>";
            selected = false;
        }
    }

    private bool animating = false;
    private IEnumerator OpenAnimation()
    {
        animating = true;
        /*
        * 1. Set the default colors at start based on quest
        * 2. Use animator animation for all movement related things
        * 3. If needed, use Color.Lerp for the color fade in animation
        * 4. For the initial "bar fill" animation, based on the data of the quest
        *    have the position values Lerp to their proper values from start.
        */

        // 1. Set default colors (Done beforehand)
        if (selected)
        {
            text_header.text = $"<color=#{ColorUtility.ToHtmlStringRGB(color_main)}>{header_name}</color>";
        }
        else
        { // Only selected quests get color
            text_header.text = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>{header_name}</color>";
        }

        // 2. Start the animator
        animator.Play("SmallQuest_Open");

        // 3. Color fade in /w Lerp
        Color start = Color.black;
        Color end = color_main;
        Color endDark = color_dark;

        // 3.5 The bar fill
        float barEndAmount = image_bar_main.fillAmount;
        image_bar_main.fillAmount = 0f; // Start a 0%
        float barTextStartX = -292f;
        float barTextMaxEnd = -79.5f;
        text_bar.rectTransform.anchoredPosition += new Vector2(barTextStartX, 0); // Start on the left side

        float elapsedTime = 0f;
        float duration = 0.5f;
        while (elapsedTime < duration) // Black -> Main Color
        {
            // Do lerp
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);
            Color darkLerp = Color.Lerp(start, endDark, elapsedTime / duration);
            Color light2Main = Color.Lerp(color_bright, end, elapsedTime / duration);
            Color dark2Main = Color.Lerp(endDark, end, elapsedTime / duration);

            // And assign colors
            foreach (var Image in image_border_bars)
            {
                Image.color = lerp;
            }
            image_main_borders.color = lerp;
            text_amount.color = lerp;
            text_description.color = lerp;
            // Black -> Dark
            text_bar.color = darkLerp;
            image_bar_background.color = darkLerp;
            // Some of the bar uses light -> main
            image_bar_side.color = light2Main;
            image_bar_main.color = light2Main;
            text_description.color = light2Main;

            // Header only gets color if its selected
            if (selected)
            {
                image_header_backer.color = Color.Lerp(end, start, elapsedTime / duration);
            }
            else
            {
                image_header_backer.color = Color.Lerp(Color.gray, start, elapsedTime / duration);
            }

            // ==== THE BAR ====
            // We need to:
            // 1. Lerp the bar from 0% fill to whatever fill it needs to be
            // 2. Move the fill % text along with the fill
            image_bar_main.fillAmount = Mathf.Lerp(0f, barEndAmount, elapsedTime / duration);
            // The max is -79.5f, we need to find out where in between it should be
            float interpolate = barTextStartX + (barTextMaxEnd - barTextStartX) * Mathf.Clamp01(barEndAmount);
            float x = Mathf.Lerp(barEndAmount, interpolate, elapsedTime / duration);
            text_bar.rectTransform.anchoredPosition += new Vector2(x, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        animating = false;
    }

    public void HoverEnter()
    {
        if (!animating)
        { // Just highlight the borders
            foreach (var Image in image_border_bars)
            {
                Image.color = color_bright;
            }
        }
    }

    public void HoverExit()
    {
        if (!animating)
        { // Reset the borders
            foreach (var Image in image_border_bars)
            {
                Image.color = color_main;
            }
        }
    }
}

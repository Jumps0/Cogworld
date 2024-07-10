using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISmallQuest : MonoBehaviour
{
    [Header("Info")]
    public Quest quest;

    [Header("References")]
    [SerializeField] private Animator animator;
    /* -- PLAN FOR ANIMATION --
    * 1. Set the default colors at start based on quest
    * 2. Use animator animation for all movement related things
    * 3. If needed, use Color.Lerp for the color fade in animation
    * 4. For the initial "bar fill" animation, based on the data of the quest
    *    have the position values Lerp to their proper values from start.
    */

    // Header
    [SerializeField] private TextMeshProUGUI text_header;
    [SerializeField] private Image image_header_backer;
    //
    [SerializeField] private List<Image> image_border_bars;
    // Progress Bar
    private Image image_bar_side;
    private Image image_bar_main;
    private Image image_bar_background;
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
    #region Colors
    public Color color_main = Color.white;
    public Color color_bright = Color.white;
    public Color color_dark = Color.white;
    //
    public Color c_orange1;
    public Color c_orange2;
    public Color c_orange3;
    //
    public Color c_blue1;
    public Color c_blue2;
    public Color c_blue3;
    //
    public Color c_yellow1;
    public Color c_yellow2;
    public Color c_yellow3;
    //
    public Color c_red1;
    public Color c_red2;
    public Color c_red3;
    //
    public Color c_purple1;
    public Color c_purple2;
    public Color c_purple3;
    //
    public Color c_green1;
    public Color c_green2;
    public Color c_green3;
    //
    public Color c_gray1;
    public Color c_gray2;
    public Color c_gray3;
    #endregion

    public void Init(Quest quest)
    {
        this.quest = quest;
        QuestObject info = quest.info;

        switch (info.rank) // Set the primary colors based on difficulty
        {
            case QuestRank.Default:
                Debug.LogWarning($"{quest} ({info} - {info.Id}) did not get a set rank! Visuals will not be properly set.");
                break;
            case QuestRank.Easy: // Green
                color_main = c_green1;
                color_bright = c_green2;
                color_dark = c_green3;
                break;
            case QuestRank.Medium: // Blue
                color_main = c_blue1;
                color_bright = c_blue2;
                color_dark = c_blue3;
                break;
            case QuestRank.Hard: // Orange
                color_main = c_orange1;
                color_bright = c_orange2;
                color_dark = c_orange3;
                break;
            case QuestRank.Difficult: // Red
                color_main = c_red1;
                color_bright = c_red2;
                color_dark = c_red3;
                break;
            case QuestRank.Expert: // Purple
                color_main = c_purple1;
                color_bright = c_purple2;
                color_dark = c_purple3;
                break;
            case QuestRank.Legendary: // Yellow
                color_main = c_yellow1;
                color_bright = c_yellow2;
                color_dark = c_yellow3;
                break;
        }

        // Then assign the set colors
        image_bar_side.color = color_main;
        image_bar_main.color = color_main;
        image_bar_background.color = color_dark;

        // Do the opening animation
        animator.Play("");
    }

    public void SetProgressBar()
    {
        // We need to parse the quest steps into a number. Sometimes this may just be one, but sometimes its an actual number.

        QuestObject info = quest.info;

        int max = info.actions.amount;
        float current = quest.value;

        switch (info.type)
        {
            case QuestType.Default:
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

        // Set the text
        text_amount.text = $"{current}/{max}";

        // Set the percent
        float percent = (current / max);
        image_bar_main.fillAmount = percent;
        text_bar.text = $"{Mathf.RoundToInt(percent*100)}%";
        // We also need to make sure the % text is lined up next to the end of the progress bar.


    }

    public void Select()
    {
        QuestManager.inst.SelectQuest(this);
    }

    public void Unselect()
    {
        QuestManager.inst.UnselectQuest(this);
    }
}

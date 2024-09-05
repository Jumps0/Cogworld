using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIQuestReward : MonoBehaviour
{
    [Header("Details")]
    private Quest info;
    private ItemObject itemReward;
    private int matterRewards;
    [SerializeField] private List<Color> colors = new List<Color>();

    [Header("UI")]
    [SerializeField] private Image image_icon;
    [SerializeField] private TextMeshProUGUI text_name;
    //
    [SerializeField] private Image image_border;
    [SerializeField] private Image image_icon_border;

    public void Init(Quest q, List<Color> colors, ItemObject ir, int mr)
    {
        info = q;
        itemReward = ir;
        matterRewards = mr;
        this.colors = colors;

        // And set display based on rewards
        int id = 0;
        if(mr > 0 ) // Matter reward
        {
            text_name.text = $"{mr} Matter";
            id = 17;
        }
        else if(ir != null) // Item reward
        {
            text_name.text = ir.data.itemData.itemName;
            id = ir.data.Id;
        }
        else
        {
            Debug.LogWarning($"No reward set!");
        }
        image_icon.sprite = InventoryControl.inst._itemDatabase.Items[id].inventoryDisplay;
        image_icon.color = InventoryControl.inst._itemDatabase.Items[id].itemColor;

        // Then modify colors based on what we are given
        image_border.color = colors[0];
        image_icon_border.color = colors[0];
        text_name.color = colors[1];

        // Then do the opening animation
        StartCoroutine(OpeningAnimation());
        TypeOutAnimation();
    }

    private IEnumerator OpeningAnimation()
    {
        // 1. Stretch the entire object on the x axis from 0 -> 1
        float elapsedTime = 0f;
        float duration = 0.5f;
        while (elapsedTime < duration)
        {
            this.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1, Mathf.Lerp(0, 1, elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private List<Coroutine> typeout = new List<Coroutine>();
    private void TypeOutAnimation()
    {
        Color start = colors[2]; // Dark
        Color end = colors[1]; // Bright
        Color highlight = colors[0]; // Main
        string text = text_name.text;

        List<string> strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.35f / text.Length;

        foreach (string s in strings)
        {
            typeout.Add(StartCoroutine(HF.DelayedSetText(text_name, s, delay += perDelay)));
        }
    }

    private Coroutine cr_claimed = null;
    /// <summary>
    /// Go from full colors to grayed out
    /// </summary>
    public void ClaimedAnimation(List<Color> grayed)
    {
        cr_claimed = StartCoroutine(IEClaimedAnimation(grayed));
    }

    private IEnumerator IEClaimedAnimation(List<Color> grayed)
    {
        Color dark = colors[2], dark_end = grayed[2]; // Dark
        Color bright = colors[1], bright_end = grayed[1]; // Bright
        Color main = colors[0], main_end = grayed[0]; // Main

        // We are just gonna lerp from the current colors to the grayed out colors
        float elapsedTime = 0f;
        float duration = 0.25f;
        while (elapsedTime < duration)
        {
            image_icon_border.color = Color.Lerp(main, main_end, elapsedTime / duration);
            image_border.color = Color.Lerp(main, main_end, elapsedTime / duration);
            Debug.Log($"A: {text_name.color}");
            text_name.color = Color.Lerp(bright, bright_end, elapsedTime / duration);
            image_icon.color = Color.Lerp(bright, bright_end, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"F: {text_name.color}");
    }

    private void OnDestroy()
    {
        if (cr_claimed != null)
        {
            StopCoroutine(cr_claimed);
        }

        foreach (Coroutine e in typeout)
        {
            StopCoroutine(e);
        }
    }
}

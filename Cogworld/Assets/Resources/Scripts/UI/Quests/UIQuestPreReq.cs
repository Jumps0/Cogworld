using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIQuestPreReq : MonoBehaviour
{
    [Header("Details")]
    private Quest quest;
    [SerializeField] private List<Color> colors = new List<Color>();

    [Header("UI")]
    [SerializeField] private Image image_icon;
    [SerializeField] private TextMeshProUGUI text_name;
    //
    [SerializeField] private Image image_border;
    [SerializeField] private Image image_icon_border;

    public void Init(Quest q, List<Color> colors)
    {
        quest = q;
        this.colors = colors;

        // Set UI
        text_name.text = quest.info.displayName;
        image_icon.sprite = quest.info.sprite;

        // Then modify colors based on what we are given
        image_border.color = colors[0];
        image_icon_border.color = colors[0];
        text_name.color = colors[1];

        // Then do the opening animation
        StartCoroutine(OpeningAnimation());
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
}

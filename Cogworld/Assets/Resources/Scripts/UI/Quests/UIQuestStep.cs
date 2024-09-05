using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Image = UnityEngine.UI.Image;

public class UIQuestStep : MonoBehaviour
{
    [Header("Details")]
    private Quest info;
    private GameObject stepReference;
    private int stepInSequence;
    [SerializeField] private List<Color> colors = new List<Color>();

    [Header("UI")]
    [SerializeField] private GameObject ui_check;
    [SerializeField] private TextMeshProUGUI text_main;
    [SerializeField] private Image ui_backer;
    [SerializeField] private Image check_backer;

    public void Init(int stepInSequence, Quest q, GameObject step, List<Color> colors)
    {
        this.stepInSequence = stepInSequence;
        info = q;
        stepReference = step;
        this.colors = colors;

        // Set the colors
        text_main.color = colors[1];
        ui_backer.color = colors[0];
        check_backer.color = colors[0];

        // And then set the UI based on the step
        bool stepComplete = info.completedSteps[stepInSequence];
        string stepDescription = stepReference.GetComponent<QuestStep>().stepDescription;

        // Set the text
        text_main.text = stepDescription;
        // And add the progress amount if needed
        string progress_text = "";
        if (stepReference.GetComponent<QS_MeetActor>()) // Don't care since its only 1 location
        {
            //
        }
        else if (stepReference.GetComponent<QS_GoToLocation>()) // Don't care since its only 1 location
        {
            //
        }
        else if (stepReference.GetComponent<QS_DestroyThing>()) // Usually 0/1 but could be more so make sure to check
        {
            if (stepReference.GetComponent<QS_DestroyThing>().a_max > 1)
            {
                progress_text = $"({stepReference.GetComponent<QS_DestroyThing>().a_progress}/{stepReference.GetComponent<QS_DestroyThing>().a_max})";
            }
        }
        else if (stepReference.GetComponent<QS_CollectItem>()) // Usually just 0/1 so we dont care
        {
            //
        }
        else if (stepReference.GetComponent<QS_KillBots>()) // This is actually has a number we want to show
        {
            progress_text = $"({stepReference.GetComponent<QS_KillBots>().a_progress}/{stepReference.GetComponent<QS_KillBots>().a_max})";
        }
        text_main.text += progress_text;
        // Enable or Disable the checkmark
        ui_check.SetActive(stepComplete);

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

    private List<Coroutine> coroutines = new List<Coroutine>();

    private void TypeOutAnimation()
    {
        Color start = colors[2]; // Dark
        Color end = colors[1]; // Bright
        Color highlight = colors[0]; // Main
        string text = text_main.text;

        List<string> strings = HF.SteppedStringHighlightAnimation(text, highlight, start, end);

        // Animate the strings via our delay trick
        float delay = 0f;
        float perDelay = 0.35f / text.Length;

        foreach (string s in strings)
        {
            coroutines.Add(StartCoroutine(HF.DelayedSetText(text_main, s, delay += perDelay)));
        }
    }

    private void OnDestroy()
    {
        foreach (var s in coroutines)
        {
            StopCoroutine(s);
        }
    }
}

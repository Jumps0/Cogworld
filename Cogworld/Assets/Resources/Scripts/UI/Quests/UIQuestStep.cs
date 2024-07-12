using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIQuestStep : MonoBehaviour
{
    [Header("Details")]
    private Quest info;
    private GameObject stepReference;
    [SerializeField] private List<Color> colors = new List<Color>();

    [Header("UI")]
    [SerializeField] private GameObject ui_check;
    [SerializeField] private TextMeshProUGUI text_main;
    [SerializeField] private Image ui_backer;
    [SerializeField] private Image check_backer;

    public void Init(Quest q, GameObject step, List<Color> colors)
    {
        info = q;
        stepReference = step;
        this.colors = colors;

        // Set the colors
        text_main.color = colors[1];
        ui_backer.color = colors[0];
        check_backer.color = colors[0];

        // And then set the UI based on the step, will need to do some parsing because there are multiple things the player may need to do
        bool stepComplete = false;
        string stepDescription = "";

        // TODO: MEGA PARSE

        // Set the text
        text_main.text = stepDescription;
        // Enable or Disable the checkmark
        ui_check.SetActive(stepComplete);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDataGenericDetail : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI primary_text;
    // Secondary
    public GameObject secondaryParent;
    public TextMeshProUGUI valueA_text;
    public TextMeshProUGUI secondary_text;
    // - Variable Box
    public GameObject variableBox;
    public Image variableB_image;
    public TextMeshProUGUI variableB_text;
    // - Box Bar
    public GameObject boxBarParent;
    public List<GameObject> mainBoxes;
    public List<GameObject> secondaryBoxes;

    [Header("Colors")] // For the boxes. Other stuff uses colors from UIManager
    public Color b_green;
    public Color b_yellow;
    public Color b_red;


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
        yield return null;
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

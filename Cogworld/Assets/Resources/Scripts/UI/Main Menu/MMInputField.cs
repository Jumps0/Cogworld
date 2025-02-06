// By: Cody Jackson | cody@krselectric.com
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Script containing logic for the input fields on the main menu
/// </summary>
public class MMInputField : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image image_back;
    [SerializeField] private TextMeshProUGUI text_main;
    [SerializeField] private TMP_InputField field;

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_hover;
    [SerializeField] private Color color_bright;
    [SerializeField] private Color color_text;

    private void Update()
    {
        if (this.gameObject.activeInHierarchy)
        {
            SetFocus(true);
            field.caretPosition = field.text.Length;

            if (field.isFocused)
            {
                ForceMeshUpdates();
            }

            SetFocus(true);
            field.caretPosition = field.text.Length;
        }
    }

    #region Misc Helpers
    public void SetFocus(bool active)
    {

        if (active)
        {
            field.Select();
            field.ActivateInputField();
        }
        else
        {
            field.ReleaseSelection();
            field.DeactivateInputField();
        }

    }

    public void ForceMeshUpdates()
    {
        field.textComponent.ForceMeshUpdate();
    }
    #endregion
}

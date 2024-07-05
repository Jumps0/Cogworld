using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetText : MonoBehaviour
{
    [Header("Reference")]
    public TextMeshProUGUI text;

    public void SetString(string newText)
    {
        text.text = newText;
    }
}

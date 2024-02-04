using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICycler : MonoBehaviour
{
    [Header("Assignments")]
    // -
    //
    public Image lineBar;
    public TextMeshProUGUI cycleText;
    [Tooltip("The [     ]")]
    public TextMeshProUGUI cycleFrame;
    public Button _button;
    //
    // -

    public Color defaultColor;
    public bool _cycle = false; // Default off


    public void Cycle()
    {
        _cycle = !_cycle; // Flip

        if (_cycle == true)
        {

        }
        else
        {

        }
    }
}

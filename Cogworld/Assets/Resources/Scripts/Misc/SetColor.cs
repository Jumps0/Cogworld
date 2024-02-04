using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class SetColor : MonoBehaviour
{
    [Header("Colors")]
    public Color colorA;
    public Color colorB;

    [Header("Target")]
    public Image target;

    public void SetColorA()
    {
        target.color = colorA;
    }

    public void SetColorB()
    {
        target.color = colorB;
    }

    public void SetCustomColorA(Color colorS)
    {
        target.color = colorS;
    }

    public void SetCustomColorB(Color colorS)
    {
        target.color = colorS;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDataHeader : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI mainText;

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
}

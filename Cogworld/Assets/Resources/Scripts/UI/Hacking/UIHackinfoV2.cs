using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// This script is responsible for displaying the white-gray text that appears with a white-gray highlight in the Hack Info menu.
/// </summary>
public class UIHackinfoV2 : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public string _message;
    public Color setColor;

    public Image backer;

    public void Setup(string message)
    {
        //this.GetComponent<RectTransform>().sizeDelta = (new Vector2(600, 300));

        _message = message;
        _text.color = setColor;
        _text.text = _message;
        AppearAnim();

        this.gameObject.name = _message;
    }

    public void AppearAnim()
    {
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float delay = 0.008f;

        // It's highlight accompanies it in. This goes from black, to the text color, back to black
        _text.gameObject.SetActive(true);
        backer.gameObject.SetActive(true);
        //
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 0f);
        yield return new WaitForSeconds(delay);
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 0.25f);
        yield return new WaitForSeconds(delay);
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 0.5f);
        yield return new WaitForSeconds(delay);
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 0.75f);
        yield return new WaitForSeconds(delay);
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 1f);
        yield return new WaitForSeconds(delay);
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 0.75f);
        yield return new WaitForSeconds(delay);
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 0.5f);
        yield return new WaitForSeconds(delay);
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 0.25f);
        yield return new WaitForSeconds(delay);
        backer.color = new Color(UIManager.inst.complexWhite.r, UIManager.inst.complexWhite.g, UIManager.inst.complexWhite.b, 0f);
    }

    public void ShutDown()
    {
        StartCoroutine(ShutdownAnim());
    }

    private IEnumerator ShutdownAnim()
    {

        yield return null;

        Destroy(this.gameObject);

    }
}

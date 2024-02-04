using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class UICombatPopup : MonoBehaviour
{
    [Header("Colors")]
    public Color textColor;
    public Color barColor;
    public Color edgeColor;

    [Header("References")]
    public string _message;
    public TextMeshProUGUI _text;
    public Image backing;
    public Image sideBar;
    public GameObject _parent;

    public List<GameObject> connectors = new List<GameObject>();
    public bool mouseOver;

    public void Setup(string message, GameObject set_parent, Color tColor, Color bColor, Color eColor)
    {
        _parent = set_parent;
        _message = message;
        textColor = tColor;
        barColor = bColor;
        edgeColor = eColor;

        _text.text = _message;
        backing.color = bColor;
        sideBar.color = eColor;
    }

    public void MessageIn()
    {
        // << The Animation Process >>
        // - Line appears from item
        // - Line expands into edge cap
        // - Text + Bar fades in from black
        // - (At the same time) Edge comes in from WHITE to its normal color
        //      -Line follows the same rules

        StartCoroutine(FadeIn());
    }

    public void MessageOut()
    {
        // < The Animation Process >>
        // - The Line will fade out to black
        // - The bar + edge become black for a frame
        // - The bar + edge become a darker color of the bar color, and fade out (with the text)

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color currentColor = backing.color;
        Color endColor = sideBar.color;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            sideBar.GetComponent<Image>().color = Color.Lerp(Color.white, endColor, elapsedTime); // Edge: White -> Set Color
            currentColor.a = Mathf.Lerp(0f, 1f, elapsedTime);
            backing.GetComponent<Image>().color = currentColor;
            //_text.color = currentColor;

            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        Color currentColor = sideBar.GetComponent<Image>().color;

        sideBar.GetComponent<Image>().color = Color.black;
        backing.GetComponent<Image>().color = Color.black;

        yield return new WaitForSeconds(0.25f); // Quick sequency of Black

        Color adjustment = new Color(currentColor.r, currentColor.g, currentColor.b, 0.7f);
        sideBar.GetComponent<Image>().color = adjustment;
        backing.GetComponent<Image>().color = adjustment;

        float elapsedTime = 0f;


        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            currentColor.a = Mathf.Lerp(0.7f, 0f, elapsedTime);
            backing.GetComponent<Image>().color = currentColor;
            sideBar.GetComponent<Image>().color = currentColor;
            _text.color = currentColor;
            yield return null;
        }

        this.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
    }
}

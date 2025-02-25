using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIExitPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public TextMeshProUGUI _text;
    public Image backing;
    public Image sideBar;
    [SerializeField] private Image blackCover;

    public GameObject boxing;
    public GameObject connectorLine;

    [Tooltip("The location of the parent tile for this exit.")]
    public Vector2Int _parent;

    public List<GameObject> connectors = new List<GameObject>();

    [Header("Values")]
    public string setName;
    public bool mouseOver = false;

    public void Setup(string name, WorldTile parent)
    {
        setName = name;
        _parent = parent.location;
        _text.text = setName;
        Appear();
    }

    public void Appear()
    {
        // << The Animation Process >>
        // - Line appears from item
        // - Line expands into edge cap
        // - Text + Bar fades in from black
        // - (At the same time) Edge comes in from WHITE to its normal color
        //      -Line follows the same rules

        blackCover.enabled = true;

        StartCoroutine(FadeIn());
        StartCoroutine(ConnectorExpand());
    }

    public void Disappear()
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
            SetConnectorsColor(Color.Lerp(Color.white, endColor, elapsedTime));
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
            SetConnectorsColor(currentColor);
            backing.GetComponent<Image>().color = currentColor;
            sideBar.GetComponent<Image>().color = currentColor;
            _text.color = currentColor;
            yield return null;
        }

        blackCover.enabled = false;
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    IEnumerator ConnectorExpand()
    {
        foreach (GameObject C in connectors)
        {
            C.SetActive(true);
            yield return new WaitForSeconds(0.25f);
        }
    }

    void SetConnectorsColor(Color color)
    {
        foreach (GameObject C in connectors)
        {
            C.GetComponent<Image>().color = color;
        }
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

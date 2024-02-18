using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class UIItemPopup : MonoBehaviour
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
        SetConnectorsColor(eColor);
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
        StartCoroutine(ConnectorExpand());
    }

    public void MessageOut()
    {
        // < The Animation Process >>
        // - The Line will fade out to black
        // - The bar + edge become black for a frame
        // - The bar + edge become a darker color of the bar color, and fade out (with the text)

        //StartCoroutine(FadeOut());
        NoFade();
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

        this.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void NoFade()
    {
        this.gameObject.SetActive(false);
        Destroy(gameObject);
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

    public class DrawLine : MonoBehaviour
    {
        public Transform startTransform;
        public Transform endTransform;
        public GameObject linePrefab;
        public float lineWidth = 0.1f;

        private Vector3[] linePoints;
        private int numPoints;
        private List<GameObject> lineObjects = new List<GameObject>();

        void Start()
        {
            // Get the positions of the start and end transforms
            Vector3 startPoint = startTransform.position;
            Vector3 endPoint = endTransform.position;

            // Calculate the distance between the start and end points
            float distance = Vector3.Distance(startPoint, endPoint);

            // Calculate the number of points needed to draw the line
            numPoints = Mathf.FloorToInt(distance / lineWidth) + 1;

            // Create an array to store the positions of each point on the line
            linePoints = new Vector3[numPoints];

            // Calculate the position of each point on the line
            for (int i = 0; i < numPoints; i++)
            {
                float t = (float)i / (float)(numPoints - 1);
                linePoints[i] = Vector3.Lerp(startPoint, endPoint, t);
            }

            // Create a small cube for each point on the line
            for (int i = 0; i < numPoints - 1; i++)
            {
                GameObject lineObject = Instantiate(linePrefab, linePoints[i], Quaternion.identity);
                lineObject.transform.LookAt(linePoints[i + 1]);
                lineObject.transform.localScale = new Vector3(lineWidth, lineWidth, Vector3.Distance(linePoints[i], linePoints[i + 1]));
                lineObjects.Add(lineObject);
            }
        }

        void OnDestroy()
        {
            // Destroy all line objects when the script is destroyed
            foreach (GameObject lineObject in lineObjects)
            {
                Destroy(lineObject);
            }
        }
    }
}


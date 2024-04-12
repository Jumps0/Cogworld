using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Variant of a UICombatPopup, but shorter. Doesn't have the line leading to it.
/// </summary>
public class UICombatPopupShort : MonoBehaviour
{
    [Header("References")]
    public Image image_backer;
    public Image image_edge;
    public TextMeshProUGUI main_text;

    public void Setup(string text, Color mainColor, Color edgeColor, float time = 5f)
    {
        main_text.text = text;
        image_backer.color = mainColor;
        image_edge.color = edgeColor;

        MessageOut(time);
    }

    public void MessageOut(float time = 5f)
    {
        // < The Animation Process >>
        // - The Line will fade out to black
        // - The bar + edge become black for a frame
        // - The bar + edge become a darker color of the bar color, and fade out (with the text)

        StartCoroutine(FadeOut(time));
    }

    IEnumerator FadeOut(float time = 5f)
    {
        yield return new WaitForSeconds(time); // Wait...

        Color currentColor = image_edge.GetComponent<Image>().color;

        image_edge.GetComponent<Image>().color = Color.black;
        image_backer.GetComponent<Image>().color = Color.black;

        yield return new WaitForSeconds(0.25f); // Quick sequency of Black

        Color adjustment = new Color(currentColor.r, currentColor.g, currentColor.b, 0.7f);
        image_edge.GetComponent<Image>().color = adjustment;
        image_backer.GetComponent<Image>().color = adjustment;

        float elapsedTime = 0f;


        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            currentColor.a = Mathf.Lerp(0.7f, 0f, elapsedTime);
            image_backer.GetComponent<Image>().color = currentColor;
            image_edge.GetComponent<Image>().color = currentColor;
            main_text.color = currentColor;
            yield return null;
        }

        this.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

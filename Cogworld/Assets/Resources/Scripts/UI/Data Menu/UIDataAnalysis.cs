using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDataAnalysis : MonoBehaviour
{
    [Header("References")]
    public Image image_backer;
    public Image image_A;
    public Image image_cover;
    public TextMeshProUGUI text_main;

    [Header("Colors")]
    public Color darkGreen;
    public Color green;
    public Color brightGreen;

    public void AppearAnimate()
    {
        UIManager.inst.dataMenu.data_traitBox.GetComponent<RectTransform>().position = this.transform.position + new Vector3(-75, 0); // Reposition the trait box

        image_cover.gameObject.SetActive(false);
        StartCoroutine(AppearAnimation());
    }

    private IEnumerator AppearAnimation()
    {
        StartCoroutine(ABoxAppear());
        Color lerp = Color.white;

        float delay = 0f;
        float perDelay = 0.01f;

        // This animation goes:
        // (Text color): Green -> Black -> Bright Green -> Black -> Bright Green -> Green. The first "A" is unaffected and stays bright green.

        float elapsedTime = 0f;
        float duration = 0.1f;
        while (elapsedTime < duration) // Green -> Black
        {
            lerp = Color.Lerp(green, Color.black, elapsedTime / duration);

            StartCoroutine(HF.DelayedSetText(text_main, $"<color=#{ColorUtility.ToHtmlStringRGB(lerp)}>[</color><color=#{ColorUtility.ToHtmlStringRGB(brightGreen)}>A</color><color=#{ColorUtility.ToHtmlStringRGB(lerp)}>NALYSIS]</color>"
, delay += perDelay));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        duration = 0.1f;
        while (elapsedTime < duration) // Black -> Bright Green
        {
            lerp = Color.Lerp(Color.black, brightGreen, elapsedTime / duration);

            StartCoroutine(HF.DelayedSetText(text_main, $"<color=#{ColorUtility.ToHtmlStringRGB(lerp)}>[</color><color=#{ColorUtility.ToHtmlStringRGB(brightGreen)}>A</color><color=#{ColorUtility.ToHtmlStringRGB(lerp)}>NALYSIS]</color>"
, delay += perDelay));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        duration = 0.1f;
        while (elapsedTime < duration) // Bright Green -> Black
        {
            lerp = Color.Lerp(brightGreen, Color.black, elapsedTime / duration);

            StartCoroutine(HF.DelayedSetText(text_main, $"<color=#{ColorUtility.ToHtmlStringRGB(lerp)}>[</color><color=#{ColorUtility.ToHtmlStringRGB(brightGreen)}>A</color><color=# {ColorUtility.ToHtmlStringRGB(lerp)} >NALYSIS]</color>"
, delay += perDelay));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        duration = 0.1f;
        while (elapsedTime < duration) // Black -> Bright Green
        {
            lerp = Color.Lerp(Color.black, brightGreen, elapsedTime / duration);

            StartCoroutine(HF.DelayedSetText(text_main, $"<color=#{ColorUtility.ToHtmlStringRGB(lerp)}>[</color><color=#{ColorUtility.ToHtmlStringRGB(brightGreen)}>A</color><color=# {ColorUtility.ToHtmlStringRGB(lerp)} >NALYSIS]</color>"
, delay += perDelay));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;
        duration = 0.1f;
        while (elapsedTime < duration) // Bright Green -> Green
        {
            lerp = Color.Lerp(brightGreen, green, elapsedTime / duration);

            StartCoroutine(HF.DelayedSetText(text_main, $"<color=#{ColorUtility.ToHtmlStringRGB(lerp)}>[</color><color=#{ColorUtility.ToHtmlStringRGB(brightGreen)}>A</color><color=# {ColorUtility.ToHtmlStringRGB(lerp)} >NALYSIS]</color>"
, delay += perDelay));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // And then set the text to its final state.
        string final = $"<color=#{ColorUtility.ToHtmlStringRGB(darkGreen)}>[</color>";
        final += $"<color=#{ColorUtility.ToHtmlStringRGB(brightGreen)}>A</color><color=#{ColorUtility.ToHtmlStringRGB(green)}>NALYSIS</color>";
        final += $"<color=#{ColorUtility.ToHtmlStringRGB(darkGreen)}>]</color>";
        StartCoroutine(HF.DelayedSetText(text_main, final, delay += perDelay));
    }

    public void Close()
    {
        StopCoroutine(AppearAnimation());

        StartCoroutine(CloseAnim());
    }

    private IEnumerator CloseAnim()
    {
        image_cover.gameObject.SetActive(true);

        float elapsedTime = 0f;
        float duration = 0.45f;
        while (elapsedTime < duration) // Dark green -> Black
        {

            Color color = Color.Lerp(darkGreen, Color.black, elapsedTime / duration);

            // Set the image color
            image_cover.color = color;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        Destroy(this.gameObject);
    }

    private IEnumerator ABoxAppear()
    {
        image_A.color = green;
        // Green -> Bright -> Black

        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration) // Green -> Bright
        {
            image_A.color = Color.Lerp(green, brightGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image_A.color = brightGreen;

        elapsedTime = 0f;
        duration = 0.2f;
        while (elapsedTime < duration) // Bright -> Black
        {
            image_A.color = Color.Lerp(brightGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image_A.color = Color.black;
    }

    public void MouseOver()
    {
        if(hoverAnim != null)
        {
            StopCoroutine(hoverAnim);
            hoverAnim = null;
        }
        hoverAnim = StartCoroutine(HoverAnim());

        UIManager.inst.dataMenu.data_onAnalysis = true;
    }

    public void MouseLeave()
    {
        if (leaveAnim != null)
        {
            StopCoroutine(leaveAnim);
            leaveAnim = null;
        }
        leaveAnim = StartCoroutine(LeaveAnim());

        UIManager.inst.dataMenu.data_onAnalysis = false;
        UIManager.inst.dataMenu.data_traitBox.GetComponent<UIDataTraitbox>().Close(); // Close the menu
    }

    private Coroutine hoverAnim;
    private Coroutine leaveAnim;

    private IEnumerator HoverAnim()
    {
        // Black -> Dark Green
        float elapsedTime = 0f;
        float duration = 0.25f;
        while (elapsedTime < duration)
        {
            image_backer.color = Color.Lerp(Color.black, darkGreen, elapsedTime / duration);
            image_A.color = Color.Lerp(Color.black, darkGreen, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image_backer.color = darkGreen;
        image_A.color = darkGreen;
    }

    private IEnumerator LeaveAnim()
    {
        // Dark Green -> Black
        float elapsedTime = 0f;
        float duration = 0.25f;
        while (elapsedTime < duration)
        {
            image_backer.color = Color.Lerp(darkGreen, Color.black, elapsedTime / duration);
            image_A.color = Color.Lerp(darkGreen, Color.black, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        image_backer.color = Color.black;
        image_A.color = Color.black;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}

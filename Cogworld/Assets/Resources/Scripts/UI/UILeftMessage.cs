using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILeftMessage : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public TextMeshProUGUI _hiddenText;
    public Image textBacking;
    public Image flashBacking;

    public Animator animator;

    [SerializeField] private Color appearColor;
    [SerializeField] private Color normalColor;

    public void SetText(string text)
    {
        _hiddenText.text = text;
        _text.text = text;
    }

    public void DoAnimationLoop()
    {
        animator.Play("leftMessageFlash");
        //Debug.Log("Playing from: " + animator);
    }

    public void AppearAnim()
    {
        StartCoroutine(ChangeColor());
    }

    private float elapsedTime;
    IEnumerator ChangeColor()
    {
        elapsedTime = 0f;
        Color currentColor = appearColor;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            currentColor = Color.Lerp(appearColor, normalColor, elapsedTime);
            textBacking.color = currentColor;
            yield return null;
        }

        DoAnimationLoop();
    }

    public void RemoveAnim()
    {
        _text.color = appearColor;
        textBacking.color = normalColor;
        flashBacking.color = normalColor;
        animator.enabled = false;

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        elapsedTime = 0f;
        Color currentColor = textBacking.color;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            currentColor.a = Mathf.Lerp(1f, 0f, elapsedTime);
            textBacking.color = currentColor;
            flashBacking.color = currentColor;
            yield return null;
        }

        this.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

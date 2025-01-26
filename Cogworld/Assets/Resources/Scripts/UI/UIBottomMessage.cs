using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Drawing;
using Color = UnityEngine.Color;

public class UIBottomMessage : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public Image _backing;

    public Animator animator;

    private Color setDark;
    private Color setNorm;
    private Color setText;
    

    public void Setup(string text, List<Color> colors)
    {
        _text.text = text;

        setDark = colors[0];
        setNorm = colors[1];
        setText = colors[2];
    }

    public void DoAnimationLoop()
    {
        animator.Play("bottomMessageFlash");
    }


    public void AppearAnim()
    {
        StartCoroutine(ChangeColor());
    }

    private float elapsedTime;
    IEnumerator ChangeColor()
    {
        DoAnimationLoop();
        _text.color = setText;

        elapsedTime = 0f;
        Color currentColor = setDark;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            currentColor = Color.Lerp(setDark, setNorm, elapsedTime);
            _backing.color = currentColor;
            yield return null;
        }
    }

    public void RemoveAnim()
    {
        //_text.color = setDark;
        //_backing.color = setNorm;
        animator.enabled = false;

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        elapsedTime = 0f;
        Color currentColor = _backing.color;

        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;
            currentColor.a = Mathf.Lerp(1f, 0f, elapsedTime);
            _backing.color = currentColor;
            _text.color = currentColor;
            yield return null;
        }

        this.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

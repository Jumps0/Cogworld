using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBottomMessage : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public Image _backing;

    public Animator animator;

    [Header("Colors")]
    [SerializeField] private Color blueDark;
    [SerializeField] private Color blueNorm;
    [SerializeField] private Color blueText;
    //
    [SerializeField] private Color greenDark;
    [SerializeField] private Color greenNorm;
    [SerializeField] private Color greenText;
    //
    [SerializeField] private Color redDark;
    [SerializeField] private Color redNorm;
    [SerializeField] private Color redText;
    //
    private Color setDark;
    private Color setNorm;
    private Color setText;
    

    public void Setup(string text, string color)
    {
        _text.text = text;

        switch (color)
        {
            case "Red":
                setDark = redDark;
                setNorm = redNorm;
                setText = redText;
                break;
            case "Green":
                setDark = greenDark;
                setNorm = greenNorm;
                setText = greenText;
                break;
            case "Blue":
                setDark = blueDark;
                setNorm = blueNorm;
                setText = blueText;
                break;

            default:
                break;
        }
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

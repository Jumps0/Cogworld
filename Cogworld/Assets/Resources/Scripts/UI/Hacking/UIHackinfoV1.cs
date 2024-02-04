using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// This script is responsible for the green text in the Hack Info menu. It optionally has an ACTIVE/INACTIVE indicator if an item is assigned to it.
/// </summary>
public class UIHackinfoV1 : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI _text;
    public string _message;
    public Color setColor;

    public ItemObject assignedPart; // probably a bad idea

    [Header("Active/Inactive")]
    public Image activeBase;
    public TextMeshProUGUI activeText;
    public Color activeColor;
    public Color inactiveColor;

    [SerializeField] private float textSpeed = 0.007f;

    public void Setup(string message, ItemObject part = null)
    {
        _message = message;
        _text.color = setColor;
        assignedPart = part;

        if (assignedPart != null)
        {
            activeBase.gameObject.SetActive(true);
            activeText.gameObject.SetActive(true);
            SetState(assignedPart.state);
        }
        else
        {
            activeBase.gameObject.SetActive(false);
            activeText.gameObject.SetActive(false);
        }

        this.gameObject.name = _message;

        TypeOutAnimation();
    }

    private void Update()
    {
        if(assignedPart != null)
        {
            activeBase.gameObject.SetActive(true);
            activeText.gameObject.SetActive(true);
            SetState(assignedPart.state);
        }
        else
        {
            activeBase.gameObject.SetActive(false);
            activeText.gameObject.SetActive(false);
        }
    }

    public void TypeOutAnimation()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {

        int len = _message.Length;
        _text.text = "";
        for (int i = 0; i < len; i++)
        {
            _text.text += _message[i];
            yield return new WaitForSeconds(textSpeed * Time.deltaTime);
        }
    }

    public void SetState(bool activated)
    {
        if (activated)
        {
            activeBase.color = activeColor;
            _text.color = activeColor;
            activeText.text = "ACTIVE";
        }
        else
        {
            activeBase.color = inactiveColor;
            _text.color = inactiveColor;
            activeText.text = "INACTIVE";
        }
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

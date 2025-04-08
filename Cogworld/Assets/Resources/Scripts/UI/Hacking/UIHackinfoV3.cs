using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Used for the "Chance of Detection" in the Hacking screen
/// </summary>
public class UIHackinfoV3 : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI _detectionChanceText;
    public TextMeshProUGUI _detValueText;
    public Image detectedBacker;
    public Vector2Int machine;

    [Header("Color Pallet")]
    public Color lowDetColor;
    public Color mediumDetColor;
    public Color highDetColor;
    public Color veryHighDetColor;

    public bool detected = false;
    public float detectionChance;

    [SerializeField] private float textSpeed = 0.007f;

    public void Setup(Vector2Int terminal)
    {
        machine = terminal;
        detectionChance = 1f;

        if (machine != null)
        {
            detectionChance = MapManager.inst.mapdata[terminal.x, terminal.y].machinedata.detectionChance;
        }

        // Get any possible bonuses from system sheields
        List<float> bonuses = HF.SystemShieldBonuses();

        detectionChance -= bonuses[1];

        TypeOutAnimation();
    }

    private void Update()
    {
        if (animFinished)
        {
            float detChance = detectionChance;

            if (machine != null)
            {
                detectionChance = MapManager.inst.mapdata[machine.x, machine.y].machinedata.detectionChance;
                detected = MapManager.inst.mapdata[machine.x, machine.y].machinedata.detected;
            }

            if(detectionChance > 1f)
            {
                detectionChance = 1f;
            }


            if (detected)
            {
                detectedBacker.gameObject.SetActive(true);
                // This happens in HF now
                /*
                if (!doOnce)
                {
                    // Play sound
                    AudioManager.inst.PlayMiscSpecific(AudioManager.inst.UI_Clips[39]);
                    UIManager.inst.Terminal_InitTrace();
                    doOnce = true;
                }
                */
            }
            else
            {
                detectedBacker.gameObject.SetActive(false);
            }

            if(detChance != detectionChance)
            {
                UpdateText();
            }
        }
    }

    //bool doOnce = false;
    bool animFinished = false;

    public void TypeOutAnimation()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        // First do "Chance of Detection: "
        string _message = "Chance of Detection: ";
        int len = _message.Length;
        _detectionChanceText.text = "";
        for (int i = 0; i < len; i++)
        {
            _detectionChanceText.text += _message[i];
            yield return new WaitForSeconds(textSpeed * Time.deltaTime);
        }

        this.gameObject.name = _message;

        // and then the actual value
        if (detectionChance >= 0.8f) // V. High
        {
            _message = "V. High (" + (int)(detectionChance * 100) + "%)";
        }
        else if (detectionChance < 0.8f && detectionChance >= 0.6f) // High
        {
            _message = "High (" + (int)(detectionChance * 100) + "%)";
        }
        else if (detectionChance < 0.6f && detectionChance >= 0.3f) // Medium
        {
            _message = "Medium (" + (int)(detectionChance * 100) + "%)";
        }
        else // Low
        {
            _message = "Low (" + (int)(detectionChance * 100) + "%)";
        }
        len = _message.Length;
        _detValueText.text = "";
        for (int i = 0; i < len; i++)
        {
            _detValueText.text += _message[i];
            yield return new WaitForSeconds(textSpeed * Time.deltaTime);
        }

        this.gameObject.name += _message;
        animFinished = true;
    }

    public void UpdateText()
    {
        if (!udt_active)
        {
            StartCoroutine(UpdateDetText());
        }
    }

    bool udt_active = false;
    private IEnumerator UpdateDetText()
    {
        udt_active = true;
        float delay = 0.1f;

        if (detectionChance >= 0.8f) // V. High
        {
            _detValueText.text = "V. High (" + (int)(detectionChance * 100) + "%)";
            _detValueText.color = veryHighDetColor;
        }
        else if (detectionChance < 0.8f && detectionChance >= 0.6f) // High
        {
            _detValueText.text = "High (" + (int)(detectionChance * 100) + "%)";
            _detValueText.color = highDetColor;
        }
        else if (detectionChance < 0.6f && detectionChance >= 0.3f) // Medium
        {
            _detValueText.text = "Medium (" + (int)(detectionChance * 100) + "%)";
            _detValueText.color = mediumDetColor;
        }
        else // Low
        {
            _detValueText.text = "Low (" + (int)(detectionChance * 100) + "%)";
            _detValueText.color = lowDetColor;
        }

        Color usedColor = _detValueText.color;

        // Flash the text so the player can see it better
        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.2f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.6f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 0.4f);

        yield return new WaitForSeconds(delay);

        _detValueText.color = new Color(usedColor.r, usedColor.g, usedColor.b, 1f);

        udt_active = false;
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

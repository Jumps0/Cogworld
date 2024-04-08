using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Used for the timer display next to machines the player knows are working. Not called "UIMachineTimer" because it would appear before UIManager in search.
/// </summary>
public class UITimerMachine : MonoBehaviour
{
    [Header("References")]
    public Image backer;
    public TextMeshProUGUI timer_text;

    [Header("Colors")]
    public Color c_green;
    public Color c_orange;

    [Header("Timing")]
    public int startTime;
    public int currentTime;

    public void Init(int _start)
    {
        startTime = _start;
        currentTime = _start;

        timer_text.text = _start.ToString();

        this.GetComponent<Canvas>().sortingOrder = 15;

        backer.color = c_orange;
    }

    public void Tick()
    {
        currentTime--;
        timer_text.text = currentTime.ToString();

        // Calculate the total duration
        float totalDuration = startTime - currentTime;

        // Calculate the interpolation factor
        float t = Mathf.Clamp01(currentTime / totalDuration);

        // Interpolate between orange and green based on the interpolation factor
        backer.color = Color.Lerp(c_green, c_orange, t);

        if (currentTime <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}

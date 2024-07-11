using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIQuestStep : MonoBehaviour
{
    [Header("Details")]
    private Quest info;
    private GameObject stepReference;

    public void Init(Quest q, GameObject step)
    {
        info = q;
        stepReference = step;

        // And then set the UI based on the step, will need to do some parsing because there are multiple things the player may need to do



    }
}

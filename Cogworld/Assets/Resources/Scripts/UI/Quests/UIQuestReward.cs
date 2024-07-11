using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIQuestReward : MonoBehaviour
{
    [Header("Details")]
    private Quest info;
    private Item itemReward;
    private int matterRewards;

    public void Init(Quest q, Item ir, int mr)
    {
        info = q;
        itemReward = ir;
        matterRewards = mr;

        // And set display based on rewards
    }
}

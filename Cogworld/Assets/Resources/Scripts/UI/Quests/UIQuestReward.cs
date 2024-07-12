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
    [SerializeField] private List<Color> colors = new List<Color>();

    public void Init(Quest q, List<Color> colors, Item ir, int mr)
    {
        info = q;
        itemReward = ir;
        matterRewards = mr;
        this.colors = colors;

        // And set display based on rewards
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMgr : MonoBehaviour
{
    public static TestMgr inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("References")]
    public GameObject refA;
    public GameObject refB;

    private void Start()
    {
        refA.GetComponent<UIDataHeader>().Setup("Overview");
        refA.GetComponent<UIDataHeader>().Open();

        refB.GetComponent<UIDataGenericDetail>().Setup(true, true, false, "This is a text test", Color.cyan, "", false, "", false, "STATE");
        refB.GetComponent<UIDataGenericDetail>().Open();

    }
}

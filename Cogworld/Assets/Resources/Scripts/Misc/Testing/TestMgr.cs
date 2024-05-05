using System;
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
        //StartCoroutine(Delay(2.5f));
    }

    private IEnumerator Delay(float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        /*
        refA.GetComponent<UIDataHeader>().Setup("Overview");
        refA.GetComponent<UIDataHeader>().Open();

        //refB.GetComponent<UIDataGenericDetail>().Setup(true, true, false, "This is a text test", Color.cyan, "", false, "", false, "STATE"); // Variable Box
        refB.GetComponent<UIDataGenericDetail>().Setup(false, false, false, "This is a text test", Color.white); // Basic (no secondary)
        //refB.GetComponent<UIDataGenericDetail>().Setup(true, false, true, "This is a text test", Color.white, "", false, "", false, "", 0.9f); // Bar
        refB.GetComponent<UIDataGenericDetail>().Open();
        */
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Delay());
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            refA.GetComponent<UIDataHeader>().Close();
            refB.GetComponent<UIDataGenericDetail>().Close();
        }
    }
}

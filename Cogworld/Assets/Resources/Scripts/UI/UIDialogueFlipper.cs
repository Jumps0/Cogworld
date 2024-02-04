using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDialogueFlipper : MonoBehaviour
{
    public Image _s;
    public int myNum = -2;
    private void Start()
    {
        // Parse num based on name
        string myName = this.gameObject.name;
        string[] s1 = myName.Split('(');
        string[] s2 = s1[1].Split(')');
        myNum = int.Parse(s2[0]);
    }

    // Update is called once per frame
    void Update()
    {
        if(this.transform.parent.GetComponent<UITextSpeedTest>() && this.transform.parent.GetComponent<UITextSpeedTest>().rander == myNum)
        {
            _s.color = Color.blue;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BinaryFlip : MonoBehaviour
{
    public bool active = true;
    public TextMeshProUGUI _text;

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if(!running)
                StartCoroutine(Animate());
        }
    }

    public bool running = false;

    public IEnumerator Animate()
    {
        running = true;

        yield return new WaitForSecondsRealtime(Random.Range(0.3f, 1f));

        if(_text.text == "0")
        {
            _text.text = "1";
        }
        else
        {
            _text.text = "0";
        }

        running = false;
    }
}

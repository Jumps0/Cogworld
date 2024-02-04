using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using System.Text;

public class UITextSpeedTest : MonoBehaviour
{
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI comparisonText;
    public float time;

    bool once = false;
    // Update is called once per frame
    void Update()
    {
        if (!once)
        {
            if(Input.GetKeyDown(KeyCode.Slash))
            {
                StartCoroutine(RealTestLoop());
                StartCoroutine(Loop());
            }
        }
    }

    private IEnumerator Loop()
    {
        once = true;

        while (true)
        {
            string random = "";

            for (int i = 0; i < 10; i++)
            {
                random += Random.Range(111111, 999999);
            }

            comparisonText.text = random;

            yield return new WaitForSeconds(time);
        }
    }

    string speech = "This is a test message of dialogue reveal speed. Lets see if this will work as the reveal speed increases! Alpha Bravo Charlie Delta Echo";
    public int rander = -1;

    private IEnumerator RealTestLoop()
    {
        once = true;

        // Create a list of strings to represent the reveal variants
        List<string> revealVariants = GenerateRevealVariants(speech);

        // Reveal characters over time
        /*
        foreach (string variant in revealVariants)
        {
            // Update the display text
            mainText.text = variant;

            // Wait for a fixed duration
            yield return new WaitForSeconds(time); //0.5f / revealVariants.Count
        }
        */

        /*
        int i = 0;
        while(mainText.text != speech)
        {
            mainText.text = revealVariants[i++];

            yield return new WaitForSeconds(time);
        }
        */

        /*

        // Create a StringBuilder to store the display text
        System.Text.StringBuilder displayText = new System.Text.StringBuilder(speech.Length);

        // Initialize the display text with random 0/1 characters
        for (int i = 0; i < speech.Length; i++)
        {
            displayText.Append(Random.value < 0.5f ? "0" : "1");
        }

        mainText.text = displayText.ToString();

        // Reveal characters over time
        //float revealDuration = 0.5f / (float)speech.Length;

        // Create a list of indices to reveal in a deterministic order
        List<int> revealOrder = new List<int>(speech.Length);
        for (int i = 0; i < speech.Length; i++)
        {
            revealOrder.Add(i);
        }

        // Shuffle the reveal order list
        revealOrder = revealOrder.OrderBy(x => Random.value).ToList();

        Debug.Log(revealOrder.Count + " vs " + speech.Length);

        foreach (int index in revealOrder)
        {
            // If the character is already revealed, skip to the next iteration
            if (displayText[index] == speech[index])
                continue;

            // Reveal the character
            displayText[index] = speech[index];
            mainText.text = displayText.ToString();

            //Debug.Log(Time.time + "- Character replaced: " + text.speech[index] + " Character revealed: " + displayText.ToString());

            // Wait for the specified duration
            yield return new WaitForSeconds(time);
        }
        */

        /*
        // Create a StringBuilder to store the display text
        StringBuilder displayText = new StringBuilder(speech.Length);

        List<int> revealOrder = new List<int>(speech.Length);
        for (int j = 0; j < speech.Length; j++)
        {
            revealOrder.Add(j); // Create a list of indices to reveal in a deterministic order
            displayText.Append(Random.value < 0.5f ? "0" : "1"); // Initialize the display text with random 0/1 characters
        }
        revealOrder = revealOrder.OrderBy(x => Random.value).ToList(); // Shuffle the reveal order list

        mainText.text = displayText.ToString(); // Start off random

        int i = 0;
        while (mainText.text != speech)
        {
            string _text = mainText.text;
            _text = _text.Remove(revealOrder[i], 1).Insert(revealOrder[i], speech[revealOrder[i]].ToString());

            mainText.text = _text;

            yield return new WaitForSeconds(time);
            i++;
        }
        */

        while (mainText.text != speech)
        {
            rander++;

            yield return new WaitForSeconds(time);
        }
    }

    // Function to generate reveal variants of the input text
    private List<string> GenerateRevealVariants(string originalText)
    {
        // Create a StringBuilder to store the display text
        StringBuilder displayText = new StringBuilder(originalText.Length);
        
        List<int> revealOrder = new List<int>(originalText.Length);
        for (int i = 0; i < originalText.Length; i++)
        {
            revealOrder.Add(i); // Create a list of indices to reveal in a deterministic order
            displayText.Append(Random.value < 0.5f ? "0" : "1"); // Initialize the display text with random 0/1 characters
        }
        revealOrder = revealOrder.OrderBy(x => Random.value).ToList(); // Shuffle the reveal order list

        List<string> variants = new List<string>();
        string[] arrVariants = new string[originalText.Length + 1];
        for (int i = 0; i < originalText.Length + 1; i++)
        {
            arrVariants[i] = displayText.ToString(); // Fill the array
        }

        for (int i = 0; i < arrVariants.Length; i++)
        {
            if(i != 0) // Skip the first one since its nothing revealed yet
            {
                string variant = arrVariants[i - 1]; // Start off with the previous string
                //Debug.Log(variant + " : " + revealOrder[i - 1] + " : " + originalText[revealOrder[i - 1]].ToString());
                variant = variant.Remove(revealOrder[i - 1], 1).Insert(revealOrder[i - 1], originalText[revealOrder[i - 1]].ToString()); // Replace the character

                arrVariants[i] = variant; // Modify array
            }
        }

        variants = arrVariants.ToList(); // Convert back to list

        return variants;
    }
}

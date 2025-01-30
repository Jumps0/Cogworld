using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager inst;
    public void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        float width = Screen.width;
        float height = Screen.height;

        spritefall_start = new Vector2(width * 0.9f, height + 100f);
        spritefall_end = new Vector2(width * 0.9f, 0 - 100f);

        SetupMainButtons();
    }

    private void Update()
    {
        RunSpritefall();
    }

    [Header("References")]
    [SerializeField] private List<GameObject> buttons_main = new List<GameObject>();
    [SerializeField] private Transform buttons_area;
    [SerializeField] private GameObject button_prefab;
    private List<string> button_titles = new List<string>() { "CONTINUE", "NEW GAME", "LOAD GAME", "JOIN GAME", "RECORDS", "SETTINGS", "CREDITS", "QUIT" };

    [Header("Colors")]
    [SerializeField] private Color color_main;
    [SerializeField] private Color color_bright;
    [SerializeField] private Color color_dull;

    #region Buttons
    private void SetupMainButtons()
    {
        // Create & Setup the buttons
        for (int i = 0; i < button_titles.Count; i++)
        {
            GameObject newButton = Instantiate(button_prefab, Vector2.zero, Quaternion.identity, buttons_area);
            buttons_main.Add(newButton);

            MMButton button = newButton.GetComponent<MMButton>();

            button.Setup(button_titles[i], i + 1);
        }
    }

    public void UnSelectButtons(GameObject exception)
    {
        foreach (var B in buttons_main)
        {
            if(B != exception)
            {
                MMButton button = B.GetComponent<MMButton>();

                button.Select(false);
            }
        }
    }
    #endregion

    #region Main Window
    [Header("Main Window")]
    [SerializeField] private GameObject main_window;
    private Coroutine main_borders_co;
    [SerializeField] private Image main_border;
    [SerializeField] private GameObject main_area;

    public void ButtonAction(int instruction)
    {

        switch (instruction)
        {
            case 1: // - CONTINUE
                ToggleMainWindow(true); // Open the window

                break;
            case 2: // - NEW GAME
                ToggleMainWindow(true); // Open the window

                break;
            case 3: // - LOAD GAME
                ToggleMainWindow(true); // Open the window

                break;
            case 4: // - JOIN GAME
                ToggleMainWindow(true); // Open the window

                break;
            case 5: // - RECORDS
                // ? Might be a different window
                ToggleMainWindow(false); // Close the window

                break;
            case 6: // - SETTINGS
                ToggleMainWindow(true); // Open the window

                break;
            case 7: // - CREDITS
                // ? Might be a different window
                ToggleMainWindow(false); // Close the window

                break;
            case 8: // - QUIT
                ToggleMainWindow(false); // Close the window
                QuitGame();
                break;
            default:
                break;
        }
    }

    private void ToggleMainWindow(bool state)
    {
        main_window.SetActive(state);

        if(main_borders_co != null)
        {
            StopCoroutine(main_borders_co);
        }
        StartCoroutine(MainWindowAnimation(state));
    }

    private IEnumerator MainWindowAnimation(bool state)
    {

        if (state) // Open is more complex
        {
            float delay = 0.1f;

            Color color = color_main;

            main_border.color = new Color(color.r, color.g, color.b, 0f);
            //headertext.color = new Color(color.r, color.g, color.b, 0.25f);

            yield return new WaitForSeconds(delay);

            main_border.color = new Color(color.r, color.g, color.b, 0.4f);
            //headertext.color = new Color(color.r, color.g, color.b, 0.75f);

            yield return new WaitForSeconds(delay);

            main_border.color = new Color(color.r, color.g, color.b, 0.2f);
            //headertext.color = new Color(color.r, color.g, color.b, 1f);

            yield return new WaitForSeconds(delay);

            main_border.color = new Color(color.r, color.g, color.b, 0.6f);
            //headertext.color = new Color(color.r, color.g, color.b, 0.75f);

            yield return new WaitForSeconds(delay);

            main_border.color = new Color(color.r, color.g, color.b, 0.4f);
            //headertext.color = new Color(color.r, color.g, color.b, 0.25f);

            yield return new WaitForSeconds(delay);

            main_border.color = new Color(color.r, color.g, color.b, 1f);
            //headertext.color = new Color(color.r, color.g, color.b, 0f);
        }
        else // Close is pretty simple
        {
            Color start = color_main;
            Color end = Color.black;

            main_border.color = start;

            float elapsedTime = 0f;
            float duration = 0.45f;
            while (elapsedTime < duration)
            {
                main_border.color = Color.Lerp(start, end, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            main_border.color = end;
        }
    }

    #endregion

    #region Settings
    [Header("Settings")]
    public ScriptableSettings settingsObject;

    // ?

    #endregion

    #region (Ambient) Spritefall
    [Header("Ambient Sprites")]
    public BotDatabaseObject bots;
    public GameObject spritefall_prefab;
    private float spritefall_time = 35f;
    private Coroutine spritefall_co = null;
    [SerializeField] private Vector2 spritefall_start;
    [SerializeField] private Vector2 spritefall_end;
    [SerializeField] private Transform spritefall_area;
    [SerializeField] private Color spritefall_color;

    private void RunSpritefall()
    {
        if(spritefall_co == null)
        {
            spritefall_co = StartCoroutine(Spritefall(spritefall_start, spritefall_end));
        }
    }

    private IEnumerator Spritefall(Vector2 start, Vector2 finish)
    {
        // Instantiate the GameObject
        var obj = Instantiate(spritefall_prefab, start, Quaternion.identity, spritefall_area); // Instantiate

        // Randomly set the sprite
        int random = Random.Range(0, bots.Bots.Length);
        obj.GetComponent<Image>().sprite = bots.Bots[random].displaySprite;

        // and color (this should be changed later to be dependent on bot class)
        obj.GetComponent<Image>().color = new Color(spritefall_color.r, Random.Range(0, 255f) / 255f, 0f);

        // Set it to the top of the move area
        obj.transform.position = start;

        // Smoothly move it to the bottom of the move area.
        float elapsedTime = 0f;
        float duration = spritefall_time;
        while (elapsedTime < duration)
        {
            obj.transform.position = new Vector2(start.x, Mathf.Lerp(start.y, finish.y, elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = finish;

        // All done, destroy
        Destroy(obj);
        spritefall_co = null;
    }
    #endregion

    public void SwitchGameScene()
    {
        SceneManager.LoadScene("GameplayScene");
    }

    //
    // -----------------------

    public void QuitGame()
    {
        Application.Quit();
    }
}

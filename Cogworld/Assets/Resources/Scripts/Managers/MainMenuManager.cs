using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
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
        KeyboardInputDetection();
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
    [SerializeField] private Color color_white;

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
        this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_ui["OPEN_1"], 0.7f); // UI - OPEN_1

        switch (instruction)
        {
            case 1: // - CONTINUE
                ToggleRecordsWindow(false, true); // Force close any other windows
                ToggleCreditsWindow(false, true);
                ToggleQuitWindow(false, true);

                ToggleMainWindow(true); // Open the window

                break;
            case 2: // - NEW GAME
                ToggleRecordsWindow(false, true); // Force close any other windows
                ToggleCreditsWindow(false, true);
                ToggleQuitWindow(false, true);

                ToggleMainWindow(true); // Open the window

                break;
            case 3: // - LOAD GAME
                ToggleRecordsWindow(false, true); // Force close any other windows
                ToggleCreditsWindow(false, true);
                ToggleQuitWindow(false, true);

                ToggleMainWindow(true); // Open the window

                break;
            case 4: // - JOIN GAME
                ToggleRecordsWindow(false, true); // Force close any other windows
                ToggleCreditsWindow(false, true);
                ToggleQuitWindow(false, true);

                ToggleMainWindow(true); // Open the window

                break;
            case 5: // - RECORDS
                // Unique window
                ToggleMainWindow(false, true); // Close the window
                ToggleCreditsWindow(false, true);
                ToggleQuitWindow(false, true);

                ToggleRecordsWindow(true);

                break;
            case 6: // - SETTINGS
                ToggleRecordsWindow(false, true); // Force close any other windows
                ToggleCreditsWindow(false, true);
                ToggleQuitWindow(false, true);

                ToggleMainWindow(true); // Open the window

                break;
            case 7: // - CREDITS
                // Unique window
                ToggleRecordsWindow(false, true);
                ToggleMainWindow(false, true); // Close the window
                ToggleQuitWindow(false, true);

                ToggleCreditsWindow(true); // Open the credits window

                break;
            case 8: // - QUIT
                ToggleMainWindow(false, true); // Close the window
                ToggleRecordsWindow(false, true);
                ToggleCreditsWindow(false, true);

                ToggleQuitWindow(true); // Open quit window
                break;
            default:
                break;
        }
    }

    private void ToggleMainWindow(bool state, bool quickClose = false)
    {
        main_window.SetActive(state);

        if(main_borders_co != null)
        {
            StopCoroutine(main_borders_co);
        }

        if(!state && quickClose)
        {
            main_window.SetActive(false);
        }
        else
        {
            StartCoroutine(GenericWindowAnimation(main_border, state));
        }
    }

    private IEnumerator GenericWindowAnimation(Image borders, bool state)
    {

        if (state) // Open is more complex
        {
            float delay = 0.1f;

            Color color = color_main;

            borders.color = new Color(color.r, color.g, color.b, 0f);
            //headertext.color = new Color(color.r, color.g, color.b, 0.25f);

            yield return new WaitForSeconds(delay);

            borders.color = new Color(color.r, color.g, color.b, 0.4f);
            //headertext.color = new Color(color.r, color.g, color.b, 0.75f);

            yield return new WaitForSeconds(delay);

            borders.color = new Color(color.r, color.g, color.b, 0.2f);
            //headertext.color = new Color(color.r, color.g, color.b, 1f);

            yield return new WaitForSeconds(delay);

            borders.color = new Color(color.r, color.g, color.b, 0.6f);
            //headertext.color = new Color(color.r, color.g, color.b, 0.75f);

            yield return new WaitForSeconds(delay);

            borders.color = new Color(color.r, color.g, color.b, 0.4f);
            //headertext.color = new Color(color.r, color.g, color.b, 0.25f);

            yield return new WaitForSeconds(delay);

            borders.color = new Color(color.r, color.g, color.b, 1f);
            //headertext.color = new Color(color.r, color.g, color.b, 0f);
        }
        else // Close is pretty simple
        {
            Color start = color_main;
            Color end = Color.black;

            borders.color = start;

            float elapsedTime = 0f;
            float duration = 0.45f;
            while (elapsedTime < duration)
            {
                borders.color = Color.Lerp(start, end, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            borders.color = end;
        }
    }

    private IEnumerator GenericTextFlashAnimation(TextMeshProUGUI text, bool state)
    {

        if (state) // Open is more complex
        {
            float delay = 0.1f;

            Color color = color_white;

            text.color = new Color(color.r, color.g, color.b, 0.25f);

            yield return new WaitForSeconds(delay);

            text.color = new Color(color.r, color.g, color.b, 0.75f);

            yield return new WaitForSeconds(delay);

            text.color = new Color(color.r, color.g, color.b, 1f);

            yield return new WaitForSeconds(delay);

            text.color = new Color(color.r, color.g, color.b, 0.75f);

            yield return new WaitForSeconds(delay);

            text.color = new Color(color.r, color.g, color.b, 0.25f);

            yield return new WaitForSeconds(delay);

            text.color = new Color(color.r, color.g, color.b, 1f);
        }
        else // Close is pretty simple
        {
            Color start = color_main;
            Color end = Color.black;

            text.color = start;

            float elapsedTime = 0f;
            float duration = 0.45f;
            while (elapsedTime < duration)
            {
                text.color = Color.Lerp(start, end, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            text.color = end;
        }
    }

    #endregion

    #region Records
    [Header("Records")]
    [SerializeField] private GameObject records_main;
    [SerializeField] private Image records_borders;
    private Coroutine records_co;

    public void ToggleRecordsWindow(bool toggle, bool quickClose = false)
    {
        records_main.SetActive(toggle);

        if (records_co != null)
        {
            StopCoroutine(records_co);
        }

        if (!toggle && quickClose)
        {
            records_main.SetActive(false);
        }
        else
        {
            StartCoroutine(GenericWindowAnimation(records_borders, toggle));
        }
    }
    #endregion

    #region Credits
    [Header("Credits")]
    [SerializeField] private GameObject credits_main;
    [SerializeField] private Image credits_borders;
    [SerializeField] private List<Image> credits_nameplates = new List<Image>();
    [SerializeField] private List<TextMeshProUGUI> credits_primaryNames = new List<TextMeshProUGUI>();
    [SerializeField] private List<TextMeshProUGUI> credits_flairText = new List<TextMeshProUGUI>();
    private Coroutine credits_co;

    private void ToggleCreditsWindow(bool toggle, bool quickClose = false)
    {
        credits_main.SetActive(toggle);

        if (credits_co != null)
        {
            StopCoroutine(credits_co);
        }

        if (!toggle && quickClose)
        {
            credits_main.SetActive(false);
        }
        else
        {
            StartCoroutine(GenericWindowAnimation(credits_borders, toggle));

            if (toggle)
            {
                StartCoroutine(CreditsAnimateReveal());
            }
        }
    }

    private IEnumerator CreditsAnimateReveal()
    {
        // We have to animate the:
        // -Bright green "name plate" backer images
        // -Black name text
        // -Green flair text & main top text

        // 1. Set all elements to their starting state


        // ?


        // ?. Set all elements to their final state 
        foreach (var NP in credits_nameplates)
        {
            // Bright color for backers
            if(NP.gameObject.activeInHierarchy)
                NP.color = color_bright;
        }
        foreach (var PT in credits_primaryNames)
        {
            // Black color for primary names
            if (PT.gameObject.activeInHierarchy)
                PT.color = Color.black;
        }
        foreach (var FT in credits_flairText)
        {
            // Green color for flair text
            if (FT.gameObject.activeInHierarchy)
                FT.color = color_main;
        }

        yield return null;
    }
    #endregion

    #region Quit
    [Header("Quit")]
    [SerializeField] private GameObject quit_main;
    [SerializeField] private Image quit_borders;
    [SerializeField] private TextMeshProUGUI quit_text;
    [SerializeField] private MMButtonSimple quit_yes;
    [SerializeField] private MMButtonSimple quit_no;
    private Coroutine quit_co;

    private void ToggleQuitWindow(bool toggle, bool quickClose = false)
    {
        quit_main.SetActive(toggle);

        if (quit_co != null)
        {
            StopCoroutine(quit_co);
        }

        if (!toggle && quickClose)
        {
            quit_main.SetActive(false);
        }
        else
        {
            if (toggle)
            {
                quit_yes.Setup("Yes");
                quit_no.Setup("No");
            }

            StartCoroutine(GenericWindowAnimation(quit_borders, toggle));
            StartCoroutine(GenericTextFlashAnimation(quit_text, toggle));
        }
    }

    public void CancelQuitGame()
    {
        UnSelectButtons(null);

        this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_ui["CLOSE"], 0.7f); // UI - CLOSE

        ToggleQuitWindow(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region Settings
    [Header("Settings")]
    public ScriptableSettings settingsObject;

    // ?

    #endregion

    #region Keyboard Input Detection
    private void KeyboardInputDetection()
    {
        // Check for player input
        if (Keyboard.current.anyKey.wasPressedThisFrame) // FUTURE TODO: Check if player is typing in an inputfield
        {
            // Go through the primary buttons
            foreach (var O in button_titles)
            {
                int value = button_titles.IndexOf(O);
                value++; // Since the index does not equal what we are actually displaying (0 to 7 vs 1 to 8), we go up by 1
                string parsed = value.ToString();

                // Convert assigned character to KeyControl
                var keyControl = Keyboard.current[parsed.ToLower()] as KeyControl;

                if (keyControl != null && keyControl.wasPressedThisFrame)
                {
                    // Click!
                    value--; // Step it back down again
                    buttons_main[value].GetComponent<MMButton>().Click();
                    return;
                }
            }
        }
    }
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

}

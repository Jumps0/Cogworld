using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using System.Linq;


public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager inst;
    public void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        float width = Screen.width;
        float height = Screen.height;

        spritefall_start = new Vector2(width * 0.9f, height + 100f);
        spritefall_end = new Vector2(width * 0.9f, 0 - 100f);

        SetupMainButtons();
        SetupSpritewheel();
    }

    private void Update()
    {
        KeyboardInputDetection();
        ArrowNavigation();
        //RunSpritefall();
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

        SettingsClose();
        settings_parent.SetActive(false);

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
                SettingsOpen();

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
                foreach (var NP in credits_nameplates)
                {
                    if (NP.gameObject.activeInHierarchy)
                        StartCoroutine(CreditsRevealBacker(NP));
                }
                foreach (var PT in credits_primaryNames)
                {
                    if (PT.gameObject.activeInHierarchy)
                        StartCoroutine(CreditsRevealNames(PT));
                }
                foreach (var FT in credits_flairText)
                {
                    if (FT.gameObject.activeInHierarchy)
                    {
                        List<string> strings = HF.RandomHighlightStringAnimation(FT.text, color_main);
                        // Animate the strings via our delay trick
                        float delay = 0f;
                        float perDelay = 0.25f / (FT.text.Length);

                        foreach (string s in strings)
                        {
                            StartCoroutine(HF.DelayedSetText(FT, s, delay += perDelay));
                        }
                    }
                }
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

    private IEnumerator CreditsRevealBacker(Image I)
    {
        // Real simple: Black -> Bright Green
        Color start = Color.black;
        Color end = color_bright;

        I.color = start;

        float elapsedTime = 0f;
        float duration = 0.35f;
        while (elapsedTime < duration)
        {
            I.color = Color.Lerp(start, end, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        I.color = end;
    }

    private IEnumerator CreditsRevealNames(TextMeshProUGUI T)
    {
        // Bright Green -> Black
        Color start = color_bright;
        Color end = Color.black;

        T.color = start;

        float elapsedTime = 0f;
        float duration = 0.35f;
        while (elapsedTime < duration)
        {
            T.color = Color.Lerp(start, end, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        T.color = end;
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

        this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_ui["CLOSE"], 0.5f); // UI - CLOSE

        ToggleQuitWindow(false);
    }

    public void QuitGame()
    {
        UnityEngine.Application.Quit();
    }
    #endregion

    #region Settings
    [Header("Settings")]
    [SerializeField] private GameObject settings_parent;
    [SerializeField] private TextMeshProUGUI settings_explainerText;
    [SerializeField] private List<GameObject> settings_gameObjects = new List<GameObject>();
    [SerializeField] private Transform settings_area;
    [SerializeField] private Transform settings_area_alt; // If there are too many settings to display, this will be enabled to put half into.
    [SerializeField] private GameObject settings_prefab;
    [SerializeField] private GameObject settings_line_prefab;
    //
    [SerializeField] private GameObject settings_header_prefab;
    [SerializeField] private Transform settings_header_area;
    [SerializeField] private List<string> settings_header_names = new List<string>() { "GAMEPLAY", "DISPLAY", "GRAPHICS", "AUDIO", "CONTROLS" };
    [SerializeField] private string settings_header_current = "GAMEPLAY";
    [SerializeField] private List<GameObject> settings_header_objects = new List<GameObject>();
    //
    [SerializeField] private int settings_maxperlane = 20;
    public char[] alphabet;
    //
    public ScriptableSettings settingsObject;
    public ScriptablePreferences preferencesObject;

    public void SettingsOpen()
    {
        settings_parent.SetActive(true);

        // Play sound
        this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_ui["OPEN_1"], 0.7f); // UI - OPEN_1

        // Create the headers (the are currently 5)
        bool once = true;
        foreach (var H in settings_header_names)
        {
            // New object
            GameObject newHeader = Instantiate(settings_header_prefab, Vector2.zero, Quaternion.identity, settings_header_area);

            // Set them up and animate them
            newHeader.GetComponent<MMOptionBig>().Setup(H, once); // (Set "GAMEPLAY" as the active one by default)

            // Save it
            settings_header_objects.Add(newHeader);

            once = false;
        }

        // Fill based on the first one
        settings_header_current = "GAMEPLAY";
        SettingsFill(settings_header_current);

    }

    private void SettingsFill(string header)
    {
        // Destroy any existing objects
        foreach (var GO in settings_gameObjects.ToList())
        {
            Destroy(GO);
        }
        settings_gameObjects.Clear();

        // Specify setting IDs to spawn
        List<int> ids = new List<int>();
        switch (header)
        {
            case "GAMEPLAY":
                ids = new List<int>() { 
                    3, 4, 5, 6, 13, 14, 15, 16, 17, 18, 19, 20, 
                    21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 32, 
                    33, 34, 35, 36, 37, 38, 39, 40, 42, 43, 44,
                    45, 46, 47, 48, 49
                };
                break;
            case "DISPLAY":
                ids = new List<int>() { 0, 1, 2 };
                break;
            case "GRAPHICS":
                ids = new List<int>() { 31, 41 };
                break;
            case "AUDIO":
                ids = new List<int>() { 7, 8, 9, 10, 11, 12 };
                break;
            case "CONTROLS":
                // TODO: Different menu?
                break;

            default:
                break;
        }

        // Create the settings options based on the IDs
        Transform parent = settings_area;
        Transform parent_b = settings_area;
        if(ids.Count > settings_maxperlane)
        {
            settings_area_alt.gameObject.SetActive(true);
            parent_b = settings_area_alt;
        }
        else
        {
            settings_area_alt.gameObject.SetActive(false);
        }

        for (int i = 0; i < ids.Count; i++)
        {
            // Overflow check
            if(i > settings_maxperlane)
            {
                parent = parent_b;
            }

            // New object
            GameObject newSetting = Instantiate(settings_prefab, Vector2.zero, Quaternion.identity, parent);

            // Set them up and animate them
            newSetting.GetComponent<MMButtonSettings>().Setup(ids[i], alphabet[i]);

            // Save it
            settings_gameObjects.Add(newSetting);
        }
    }

    public void SettingsBigOptionClicked(MMOptionBig caller)
    {
        // Is this option the same as the current one we are displaying?
        if(caller.display == settings_header_current) { return; }

        // Play sound
        this.GetComponent<AudioSource>().PlayOneShot(AudioManager.inst.dict_ui["OPEN_1"], 0.7f); // UI - OPEN_1

        // Destroy all existing options
        foreach (var GO in settings_gameObjects.ToList())
        {
            Destroy(GO);
        }
        settings_gameObjects.Clear();

        // Refill based on caller
        SettingsFill(caller.display);

        // Change visuals of all headers
        foreach (var H in settings_header_objects)
        {
            MMOptionBig mmob = H.GetComponent<MMOptionBig>();

            if(caller == mmob)
            {
                mmob.SetAsChosen();
            }
            else
            {
                mmob.SetAsUnchosen();
            }
        }

        settings_header_current = caller.display;
    }

    private void SettingsCreateHeader(string title, Transform parent)
    {
        // New object
        GameObject newHeader = Instantiate(settings_line_prefab, Vector2.zero, Quaternion.identity, parent);

        // Set them up and animate them
        newHeader.GetComponent<MMHeaderSimple>().Setup(title);

        // Save it
        settings_gameObjects.Add(newHeader);
    }

    public void SettingsClose()
    {
        // Clear all the objects
        foreach (var GO in settings_gameObjects.ToList())
        {
            Destroy(GO);
        }
        settings_gameObjects.Clear();

        foreach (var GO in settings_header_objects.ToList())
        {
            Destroy(GO);
        }
        settings_header_objects.Clear();
    }

    public void SettingsRevealExplainerText(string text)
    {
        // No animation! Just update the text.

        settings_explainerText.gameObject.SetActive(true);

        settings_explainerText.text = text;
    }

    public void SettingsHideExplainer()
    {
        settings_explainerText.gameObject.SetActive(false);

        settings_explainerText.text = "";

    }

    [Header("Settings Detail Box")]
    public GameObject detail_main;
    [SerializeField] private Image detail_mainbacker; // The primary black image just below the main parent
    [SerializeField] private Image detail_borders; // The main GREEN borders of the window
    [SerializeField] private Transform detail_headerParent; // The parent transform holding all of the \HEADER\ objects.
    [SerializeField] private Image detail_headerBack; // Image behind the \HEADER\
    [SerializeField] private TextMeshProUGUI detail_header; // text for the \HEADER\
    [SerializeField] private Transform detail_area; // The area (black image) where objects are spawned under
    [SerializeField] private GameObject detail_prefab; // Prefabs spawned as options in the Detail Box
    [SerializeField] private GameObject detail_xbutton; // The 'X' button which needs to be at the bottom right of the window
    [SerializeField] private TextMeshProUGUI detail_headerfill; // Secret hidden text which sets the size of the detail box
    [SerializeField] private List<GameObject> detail_objects = new List<GameObject>(); // List of all prefabs spawned in the Detail Box
    private Coroutine detail_co = null;

    #region Detail Window
    public void DetailOpen(List<(string, ScriptableSettingShort)> options, string title, MMButtonSettings caller)
    {
        detail_main.SetActive(true);

        // Clear any pre-existing options
        foreach (var O in detail_objects.ToList())
        {
            Destroy(O);
        }
        detail_objects.Clear();

        // We need to position the `Detail Window` at the `Bottom Right` corner of the owner's image backer.
        Vector3[] v = new Vector3[4];
        caller.image_backer.GetComponent<RectTransform>().GetWorldCorners(v);
        // We care about v[3] (This is the bottom right corner)
        detail_main.transform.position = v[3]; // Reposition it

        // Set the title (header) based on this option's name
        string header = $"\\{title}\\";
        detail_header.text = header;
        detail_headerfill.text = header;

        // Populate the menu with the options we need
        foreach (var O in options)
        {
            string text = O.Item1;
            ScriptableSettingShort setting = O.Item2;

            GameObject newOption = Instantiate(detail_prefab, Vector2.zero, Quaternion.identity, detail_area);

            newOption.GetComponent<MMOptionSimple>().Setup(text, setting, caller);

            detail_objects.Add(newOption);
        }

        // Opener animation
        if (detail_co != null)
        {
            StopCoroutine(detail_co);
        }
        detail_co = StartCoroutine(DetailOpenAnimation());

        // Play sound
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["MODEON"]); // UI - MODEON
    }

    private IEnumerator DetailOpenAnimation()
    {
        yield return null;
        // -- Now that the window size (and position) has changed --
        // Then position the 'X' button to be at the bottom right (based on the window itself)
        Vector3[] v = new Vector3[4];
        detail_mainbacker.GetComponent<RectTransform>().GetWorldCorners(v);
        float x = v[3].x, y = v[3].y;
        Vector3 pos = new Vector3(x - 15f, y + 3.5f, 0); // Offset it a bit so its still on the window
        detail_xbutton.transform.position = pos;

        // We need to:
        // 1. Animate the header (and its backer) (Dark Green -> Bright Green (100% Transparency) -> Black (0% Transparency)
        // 2. Animate the borders (Black -> Bright)

        // (First reset all these)
        detail_mainbacker.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        detail_area.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        detail_headerBack.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        detail_header.color = new Color(color_bright.r, color_bright.g, color_bright.b, 1f);
        detail_borders.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);

        if (DOAH_co != null)
        {
            StopCoroutine(DOAH_co);
        }
        DOAH_co = StartCoroutine(DOA_Header()); // Split off because its tricky to put all this in one loop

        float elapsedTime = 0f;
        float duration = 0.45f;

        Color start = color_dull, end = color_bright;

        detail_borders.color = start;
        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            detail_borders.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        detail_borders.color = end;
    }

    private Coroutine DOAH_co;
    private IEnumerator DOA_Header()
    {
        // Animate the header (and its backer) (Dark Green -> Bright Green -> Black
        detail_header.color = color_bright; // NO CHANGE

        // Dark Green -> Bright Green
        float elapsedTime = 0f;
        float duration = 0.25f;

        Color start = color_dull, end = color_bright;

        detail_headerBack.color = start;
        while (elapsedTime < duration)
        {
            Color lerp = Color.Lerp(start, end, elapsedTime / duration);

            detail_headerBack.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        detail_headerBack.color = end;

        // Bright Green -> Black
        elapsedTime = 0f;
        duration = 0.25f;

        detail_headerBack.color = start;
        while (elapsedTime < duration) // Empty -> Green
        {
            Color lerp = Color.Lerp(color_bright, Color.black, elapsedTime / duration);

            detail_headerBack.color = lerp;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        detail_headerBack.color = Color.black;
    }

    public void DetailShutterAllOptions(MMOptionSimple picked)
    {
        foreach (var DO in detail_objects)
        {
            MMOptionSimple mmos = DO.GetComponent<MMOptionSimple>();

            mmos.RemoveMe(mmos == picked);
        }
    }

    public void DetailClose()
    {
        // Play sound
        AudioManager.inst.CreateTempClip(Vector3.zero, AudioManager.inst.dict_ui["CLOSE"]); // UI - CLOSE

        if (detail_co != null)
        {
            StopCoroutine(detail_co);
        }
        detail_co = StartCoroutine(DetailCloseAnimation());
    }

    private IEnumerator DetailCloseAnimation()
    {
        // Destroy all the objects
        foreach (GameObject obj in detail_objects.ToList())
        {
            obj.GetComponent<MMOptionSimple>().RemoveMe();
        }
        detail_objects.Clear();

        // Quick border animation
        // - Quickly change ALL transparency to 0%
        float elapsedTime = 0f;
        float duration = 0.25f;

        detail_mainbacker.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
        detail_area.GetComponent<Image>().color = new Color(color_dull.r, color_dull.g, color_dull.b, 1f);
        detail_headerBack.GetComponent<Image>().color = new Color(color_dull.r, color_dull.g, color_dull.b, 1f);
        detail_header.color = new Color(color_dull.r, color_dull.g, color_dull.b, 1f);
        detail_borders.GetComponent<Image>().color = new Color(color_dull.r, color_dull.g, color_dull.b, 1f);

        while (elapsedTime < duration)
        {
            float lerp = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            detail_mainbacker.GetComponent<Image>().color = new Color(0f, 0f, 0f, lerp);
            detail_area.GetComponent<Image>().color = new Color(color_dull.r, color_dull.g, color_dull.b, lerp);
            detail_headerBack.GetComponent<Image>().color = new Color(color_dull.r, color_dull.g, color_dull.b, lerp);
            detail_header.color = new Color(color_dull.r, color_dull.g, color_dull.b, lerp);
            detail_borders.GetComponent<Image>().color = new Color(color_dull.r, color_dull.g, color_dull.b, lerp);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        detail_mainbacker.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        detail_area.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        detail_headerBack.GetComponent<Image>().color = new Color(color_dull.r, color_dull.g, color_dull.b, 0f);
        detail_header.color = new Color(color_dull.r, color_dull.g, color_dull.b, 0f);
        detail_borders.GetComponent<Image>().color = new Color(color_dull.r, color_dull.g, color_dull.b, 0f);

        yield return null;

        // Disable the box (no animation)
        detail_main.SetActive(false);
    }

    #endregion

    /// <summary>
    /// Applies all current settings from the SObject. More basic since not much actually changes in the main menu.
    /// Full effect is in GlobalSettings.cs
    /// </summary>
    public void ApplySettingsSimple()
    {
        // Not much here to do since this is just the main menu.
        // - FONT -

        // - FULLSCREEN -

        // - AUDIO (5) -
        

    }
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

    private void ArrowNavigation()
    {
        if (settings_area.gameObject.activeInHierarchy)
        {
            if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                // What is the current index of the selected header?
                int index = settings_header_names.IndexOf(settings_header_current);

                // We want to go left (decrease)
                // Are we at 0 now? (If so loop around to the top)
                if(index == 0)
                {
                    // Goto highest in the list
                    SettingsBigOptionClicked(settings_header_objects[settings_header_objects.Count - 1].GetComponent<MMOptionBig>());
                }
                else // We can still go down more
                {
                    SettingsBigOptionClicked(settings_header_objects[index - 1].GetComponent<MMOptionBig>());
                }
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                // What is the current index of the selected header?
                int index = settings_header_names.IndexOf(settings_header_current);

                // We want to go right (increase)
                // Are we at the top now? (If so loop around to the bottom)
                if (index == settings_header_objects.Count - 1)
                {
                    // Goto lowest in the list
                    SettingsBigOptionClicked(settings_header_objects[0].GetComponent<MMOptionBig>());
                }
                else // We can still go up more
                {
                    SettingsBigOptionClicked(settings_header_objects[index + 1].GetComponent<MMOptionBig>());
                }
            }
        }
    }

    /// <summary>
    /// aka the ESCAPE key
    /// </summary>
    /// <param name="value"></param>
    public void OnQuit(InputValue value)
    {
        // TODO, check to close various menus/submenus
        if (detail_main.gameObject.activeInHierarchy)
        {
            DetailClose();
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

    #region (Ambient) Spritewheel
    [Header("Spritewheel")]
    [SerializeField] private Transform spritewheel_orbit;
    [SerializeField] private int spritewheel_max = 10;
    [SerializeField] private float spritewheel_speed = 5f;
    [SerializeField] private float spritewheel_radius = 290f;
    [SerializeField] private List<GameObject> spritewheel_objects = new List<GameObject>();
    [SerializeField] private List<(Sprite, Color)> spritewheel_sprites = new List<(Sprite, Color)>();
    [SerializeField] private GameObject spritewheel_prefab;

    private void SetupSpritewheel()
    {
        // We need to spawn in and pick X amount of bots to spawn in
        spritewheel_sprites.Clear();

        // Randomly pick sprites
        while(spritewheel_sprites.Count < spritewheel_max && spritewheel_sprites.Count <= bots.Bots.Length)
        {
            BotObject botInfo = bots.Bots[Random.Range(0, bots.Bots.Length)];
            Sprite botSprite = botInfo.displaySprite;
            Color botColor = botInfo.idealColor;

            if (!spritewheel_sprites.Contains((botSprite, botColor)))
            {
                spritewheel_sprites.Add((botSprite, botColor));
            }
        }

        // Create the objects

        // Clean up any pre-existing
        foreach (var S in spritewheel_objects)
        {
            Destroy(S.gameObject);
        }
        spritewheel_objects.Clear();

        // Evenly space out the objects in a circle
        float angleStep = 360f / spritewheel_sprites.Count;

        for (int i = 0; i < spritewheel_sprites.Count; i++)
        {
            // Create new object and set up its values
            GameObject newSprite = Instantiate(spritewheel_prefab, Vector3.zero, Quaternion.identity, spritewheel_orbit);
            Image sr = newSprite.GetComponent<Image>();
            sr.sprite = spritewheel_sprites[i].Item1;
            sr.color = spritewheel_sprites[i].Item2;

            // Position the sprite in a circular orbit
            float angle = i * angleStep;
            Vector3 offset = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle) * spritewheel_radius, Mathf.Sin(Mathf.Deg2Rad * angle) * spritewheel_radius, 0);
            newSprite.transform.position = spritewheel_orbit.position + offset;

            // Add it to the list
            spritewheel_objects.Add(newSprite);
        }

        StartCoroutine(SpritewheelOrbit());
    }

    private IEnumerator SpritewheelOrbit()
    {
        while (true)
        {
            for (int i = 0; i < spritewheel_objects.Count; i++)
            {
                // Calculate the new angle based on time and orbit speed
                float angle = Time.time * spritewheel_speed + i * (360f / spritewheel_sprites.Count);
                Vector3 offset = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle) * spritewheel_radius, Mathf.Sin(Mathf.Deg2Rad * angle) * spritewheel_radius, 0);

                // Update the position of each sprite
                spritewheel_objects[i].transform.position = spritewheel_orbit.position + offset;
            }

            yield return null; // Wait until the next frame
        }
    }

    #endregion

    public void SwitchGameScene()
    {
        SceneManager.LoadScene("GameplayScene");
    }

    //
    // -----------------------

}

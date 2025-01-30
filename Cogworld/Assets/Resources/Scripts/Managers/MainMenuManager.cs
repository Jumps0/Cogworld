using System.Collections;
using System.Collections.Generic;
using TMPro;
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
}

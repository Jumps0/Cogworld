using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

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
    }

    private void Update()
    {
        RunSpritefall();
    }

    [Header("References")]
    public GameObject titleRef;
    public GameObject helpRef;
    public GameObject buttonsRef;

    public GameObject saveDataRef;
    public GameObject saveDataAreaRef;
    public TextMeshProUGUI saveText;


    // ~~~ Level Loading ~~~
    //

    public void ResumePreviousSave()
    {




        SwitchGameScene();
    }

    public void StartNewGame()
    {



        SwitchGameScene();
    }

    public void ShowSaveData()
    {

    }

    public void HideSaveData()
    {

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


    public void ShowTitle()
    {
        titleRef.SetActive(true);
    }

    public void HideTitle()
    {
        titleRef.SetActive(false);
    }

    public void ShowHelp() 
    { 
        helpRef.SetActive(true);
    }

    public void HideHelp()
    {
        helpRef.SetActive(false);
    }

    public void OpenOptions()
    {

    }

    public void CloseOptions()
    {

    }


    [Header("Ambient Sprites")]
    public BotDatabaseObject bots;
    public GameObject spritefall_prefab;
    private float spritefall_time = 20f;
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
        obj.GetComponent<Image>().sprite = bots.Bots[Random.Range(0, bots.Bots.Length)].displaySprite;

        // and color (random from yellow to pink-ish red)
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

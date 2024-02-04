using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager inst;
    public void Awake()
    {
        inst = this;
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

    
    
}

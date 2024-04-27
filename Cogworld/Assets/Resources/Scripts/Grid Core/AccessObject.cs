using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Used by tiles that give access to a new level. Ex: BRANCH ACCESS, MAIN ACCESS.
/// </summary>
public class AccessObject : MonoBehaviour
{
    [Header("Values")]
    public int targetDestination;
    public string destName;
    public bool isBranch;
    [Tooltip("Does the player know where this leads? If not will display as '???'")]
    public bool playerKnowsDestination;

    public int locX;
    public int locY;

    public bool isExplored;
    public bool isVisible;

    public GameObject blackBacker;

    public void Setup(int target, bool branch)
    {
        targetDestination = target;
        isBranch = branch;

        if (branch)
        {
            this.GetComponent<SpriteRenderer>().sprite = MiscSpriteStorage.inst.accessSprites[1];
        }
        else
        {
            this.GetComponent<SpriteRenderer>().sprite = MiscSpriteStorage.inst.accessSprites[0];
        }
        if (target == 3 || target == 4) // DSF or GARRISON
        {
            this.GetComponent<SpriteRenderer>().sprite = MiscSpriteStorage.inst.accessSprites[2];
        }

        QueryName();
    }

    private void Update()
    {
        CheckVisibility();
    }

    private void CheckVisibility()
    {

        if (isVisible)
        {
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else if (isExplored && isVisible)
        {
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else if (isExplored && !isVisible)
        {
            this.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        else if (!isExplored)
        {
            this.GetComponent<SpriteRenderer>().color = Color.black;
        }
    }

    // FLAG - UPDATE NEW LEVELS

    // - Destination ID Guide -
    //
    // 0 - Materials
    // 1 - Lower Caves
    // 2 - Storage
    // 3 - DSF
    // 4 - Garrison
    // 5 - Factory
    // 6 - Extension
    // 7 - Upper Caves
    // 8 - Research
    // 9 - Access
    // 10 - Command
    // 11 - Armory
    // 12 - Archies
    // 13 - Zhirov
    // 14 - Data Miner
    // 15 - Architect
    // 16 - Exiles
    // 17 - Warlord
    // 18 - Section 7
    // 19 - Testing
    // 20 - Quarantine 
    // 21 - Lab
    // 22 - Hub_04(d)
    // 23 - Zion
    // 24 - Zion Deep Caves
    // 25 - Mines
    // 26 - Recycling
    // 27 - Subcaves
    // 28 - Wastes
    // 29 - Junkyard
    //
    // Things that you also need to update when you change this:
    // MapManager: PlayAmbientMusic()
    // MapManager: ChangeMap()
    // HF:         IDbyTheme()
    // 
    // -                     -

    public void QueryName()
    {
        switch (targetDestination)
        {
            case 0:
                destName = "MATERIALS";
                break;
            case 1:
                destName = "LOWER CAVES";
                break;
            case 2:
                destName = "STORAGE";
                break;
            case 3:
                destName = "DSF";
                break;
            case 4:
                destName = "GARRISON";
                break;
            case 5:
                destName = "FACTORY";
                break;
            case 6:
                destName = "EXTENSION";
                break;
            case 7:
                destName = "UPPER CAVES";
                break;
            case 8:
                destName = "RESEARCH";
                break;
            case 9:
                destName = "ACCESS";
                break;
            case 10:
                destName = "COMMAND";
                break;
            case 11:
                destName = "ARMORY";
                break;
            case 12:
                destName = "ARCHIVES";
                break;
            case 13:
                destName = "ZHIROV";
                break;
            case 14:
                destName = "DATA MINER";
                break;
            case 15:
                destName = "ARCHITECT";
                break;
            case 16:
                destName = "EXILES";
                break;
            case 17:
                destName = "WARLORD";
                break;
            case 18:
                destName = "SECTION 7";
                break;
            case 19:
                destName = "TESTING";
                break;
            case 20:
                destName = "QUARANTINE";
                break;
            case 21:
                destName = "LAB";
                break;
            case 22:
                destName = "HUB_04(d)";
                break;
            default:
                destName = "UNKNOWN";
                break;
        }
    }

    public void InitialReveal()
    {
        bool found = false;
        // If a popup for this doesn't already exist we need to create one.
        foreach (GameObject P in UIManager.inst.exitPopups)
        {
            if (P.GetComponentInChildren<UIExitPopup>()._parent == this.gameObject)
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            UIManager.inst.CreateExitPopup(this.gameObject, destName);
            if (!MapManager.inst.firstExitFound) // If this is the first exit the player has found (on this level), display a log message.
            {
                MapManager.inst.firstExitFound = true;
                UIManager.inst.CreateNewLogMessage("EXIT=FOUND", UIManager.inst.deepInfoBlue, UIManager.inst.infoBlue, false, true);
            }
        }
    }

    private void OnMouseEnter()
    {
        if (isExplored)
        {
            bool found = false;
            // If a popup for this doesn't already exist we need to create one.
            foreach (GameObject P in UIManager.inst.exitPopups)
            {
                if (P.GetComponentInChildren<UIExitPopup>()._parent == this.gameObject)
                {
                    found = true;
                    P.GetComponentInChildren<UIExitPopup>().mouseOver = true;
                    break;
                }
            }
            //Debug.Log(found);
            if (!found)
            {
                UIManager.inst.CreateExitPopup(this.gameObject, destName);
            }
        }
    }

    void OnMouseExit()
    {
        if (isExplored)
        {
            foreach (GameObject P in UIManager.inst.exitPopups)
            {
                if (P.GetComponentInChildren<UIExitPopup>()._parent == this.gameObject)
                {
                    P.GetComponentInChildren<UIExitPopup>().mouseOver = false;
                    break;
                }
            }
        }
    }
}

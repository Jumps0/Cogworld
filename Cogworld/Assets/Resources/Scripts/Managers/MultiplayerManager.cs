using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Manager for handling important aspects of multiplayer. Exists because looking inside NetworkManager scares me.
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    /* The Host starts the server and joins as a client.
     * The Client joins the server as a client player.
     * The Server starts the game as a server without instantiating a player.
     */

    [Tooltip("Network Manager")]
    [SerializeField] private NetworkManager network;
    public static MultiplayerManager inst;
    public void Awake()
    {
        inst = this;
    }

    [Header("Test UI")]
    [SerializeField] private GameObject testui_mainbox;
    [SerializeField] private TMP_InputField testui_inputfield;
    [SerializeField] private TextMeshProUGUI testui_clienttypetext;
    public GameObject testno_prefab;
    public Transform testno_ref;

    #region Test UI
    public void TEST_Host()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        testui_clienttypetext.text = "Client type: HOST";
    }

    public void TEST_ClientJoin()
    {
        NetworkManager.Singleton.StartClient();
        testui_clienttypetext.text = "Client type: CLIENT";
    }

    public void Test_Disconnect()
    {

    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        
    }
    #endregion
}

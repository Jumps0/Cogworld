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

    #region Test UI
    public void TEST_Host()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }

    public void TEST_ClientJoin()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void Test_Disconnect()
    {

    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        
    }
    #endregion

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!network.IsClient && !network.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();

            SubmitNewPosition();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) network.StartHost();
        if (GUILayout.Button("Client")) network.StartClient();
        if (GUILayout.Button("Server")) network.StartServer();
    }

    void StatusLabels()
    {
        var mode = network.IsHost ?
            "Host" : network.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            network.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    void SubmitNewPosition()
    {
        if (GUILayout.Button(network.IsServer ? "Move" : "Request Position Change"))
        {
            if (network.IsServer && !network.IsClient)
            {
                foreach (ulong uid in network.ConnectedClientsIds)
                    network.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<NetworkPlayer>().Move(new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)));
            }
            else
            {
                var playerObject = network.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<NetworkPlayer>();
                player.Move(new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)));
            }
        }
    }
}

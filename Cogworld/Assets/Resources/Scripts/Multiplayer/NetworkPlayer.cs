using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Script for multiplayer players.
/// </summary>
public class NetworkPlayer : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            inputActions = Resources.Load<InputActionsSO>("Inputs/InputActionsSO").InputActions;

            inputActions.Player.Move.performed += OnMovePerformed;
            inputActions.Player.Move.canceled += OnMoveCanceled;
            inputActions.Player.Enter.performed += OnEnter;

            Move(new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)));
        }
    }

    /*
    public override void OnNetworkDespawn()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
    }
    */

    public void Move(Vector2 direction)
    {
        SubmitPositionRequestRpc(direction);
    }

    [Rpc(SendTo.Server)]
    void SubmitPositionRequestRpc(Vector2 direction, RpcParams rpcParams = default)
    {
        transform.position += (Vector3)direction;
        Position.Value = transform.position;
    }

    [Rpc(SendTo.Server)]
    void SpawnObjectRequestRpc(Vector2 position, RpcParams rpcParams = default)
    {
        GameObject newObj = Instantiate(MultiplayerManager.inst.testno_prefab);
        newObj.GetComponent<NetworkObject>().Spawn(true);

        MultiplayerManager.inst.testno_ref = newObj.transform;

        // (The position is de-synced because the object would need an internal network variable)
        newObj.transform.position = position;
    }

    [Rpc(SendTo.Server)]
    void DestroyTestObjectRequestRpc(RpcParams rpcParams = default)
    {
        Destroy(MultiplayerManager.inst.testno_ref.gameObject);
        // Alternatively, you can do this (where the true/false destroys it or not):
        //MultiplayerManager.inst.testno_ref.gameObject.GetComponent<NetworkObject>().Despawn(true);
    }

    void Update()
    {
        transform.position = Position.Value;

        if (!IsOwner) return;

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            SpawnObjectRequestRpc(new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)));
        }
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            DestroyTestObjectRequestRpc();
        }
    }

    #region Input
    public PlayerInputActions inputActions;
    private Vector2 moveInput;

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        moveInput = context.ReadValue<Vector2>();
        Move(moveInput);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        moveInput = Vector2.zero;
    }
    public void OnEnter(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        SpawnObjectRequestRpc(new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f)));
    }
    #endregion
}

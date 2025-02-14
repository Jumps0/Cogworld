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

            Move(new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)));
        }
    }

    public override void OnNetworkDespawn()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
    }

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

    void Update()
    {
        transform.position = Position.Value;
    }

    #region Movement
    public PlayerInputActions inputActions;
    private Vector2 moveInput;

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        moveInput = context.ReadValue<Vector2>();
        Move(moveInput);
    }
    #endregion
}

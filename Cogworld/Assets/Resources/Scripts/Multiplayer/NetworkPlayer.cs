using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
            Move(new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)));
        }
    }

    public void Move(Vector2 direction)
    {
        SubmitPositionRequestRpc(direction);
    }

    [Rpc(SendTo.Server)]
    void SubmitPositionRequestRpc(Vector2 direction, RpcParams rpcParams = default)
    {
        //var randomPosition = GetRandomPositionOnPlane();

        transform.position += (Vector3)direction;
        Position.Value = transform.position;
    }

    static Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f);
    }

    void Update()
    {
        transform.position = Position.Value;
    }
}

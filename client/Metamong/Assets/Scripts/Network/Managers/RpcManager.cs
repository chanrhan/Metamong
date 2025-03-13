using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RpcManager : NetworkBehaviour
{
    public static RpcManager Instance { get; private set; }

    private void Awake() {
        // Debug.Log("Awake RpcManager: " + OwnerClientId);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SendPacketToAll(Packet packet){
        SendPacketToAllServerRpc(packet, new ServerRpcParams());
    }

    public void SendPacketTo(Packet packet, ulong[] targetIds){
        SendPacketServerRpc(packet, targetIds, new ServerRpcParams());
    }

    public void SendPacketTo(Packet packet, ulong targetId){
        SendPacketServerRpc(packet, new ulong[]{
            targetId
        }, new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPacketToAllServerRpc(Packet packet, ServerRpcParams serverRpcParams){
        ReceivePacketClientRpc(packet, new ClientRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPacketServerRpc(Packet packet, ulong[] targetIds,  ServerRpcParams serverRpcParams){
        ReceivePacketClientRpc(packet, new ClientRpcParams{
            Send = new ClientRpcSendParams{
                TargetClientIds = targetIds
            }
        });
    }

    [ClientRpc]
    private void ReceivePacketClientRpc(Packet packet, ClientRpcParams clientRpcParams){
        Debug.Log($"Receive '{packet}', by " + ClientManager.Instance.ClientInfo.clientId);

        string name = packet.clientInfo.username;
        string msg = packet.msg;
        ChatManager.Instance.InputChat(name, msg);
    }

}

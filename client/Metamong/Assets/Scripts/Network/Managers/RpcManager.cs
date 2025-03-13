using System;
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

    public void SendPacketTo(Packet packet, ulong targetId){
        SendPacketTo(packet, new ulong[]{targetId});
    }

    public void SendPacketTo(Packet packet, ulong[] targetIds = default){
        if(packet.commandCode == ECommandCode.None){
            throw new NoCommandCodeInPacketException("No Command Code In Packet!");
        }

        SendPacketServerRpc(packet, targetIds, new ServerRpcParams());
    }


    // [ServerRpc(RequireOwnership = false)]
    // private void SendPacketToAllServerRpc(Packet packet, ServerRpcParams serverRpcParams){
    //     ReceivePacketClientRpc(packet, new ClientRpcParams());
    // }

    [ServerRpc(RequireOwnership = false)]
    private void SendPacketServerRpc(Packet packet, ulong[] targetIds,  ServerRpcParams serverRpcParams){
        ClientRpcParams clientRpcParams = new ClientRpcParams();
        if(targetIds != null){
            clientRpcParams.Send = new ClientRpcSendParams{
                TargetClientIds = targetIds
            };
        }
        ReceivePacketClientRpc(packet, clientRpcParams);
    }

    [ClientRpc]
    private void ReceivePacketClientRpc(Packet packet, ClientRpcParams clientRpcParams){
        Debug.Log($"Receive '{packet}', by " + ClientManager.Instance.ClientInfo.clientId);

        PacketReceiveHandler.DecodePacket(packet);
    }

}

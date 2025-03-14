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

    [Obsolete]
    public void SendPacketTo(Packet packet, ulong clientId){
        SendPacketTo(packet, new ulong[]{clientId});
    }

    [Obsolete]
    public void SendPacketTo(Packet packet, ulong[] targetIds = default){
        
    }

    public void SendPacketTo(Packet packet, NetworkTarget[] targets){
        PacketSendWrapper sendWrapper = new PacketSendWrapper{
            packet = packet,
            networkTargets = targets
        };
        SendPacket(sendWrapper);
    }

    
    private void SendPacket(PacketSendWrapper packetSendWrapper){
        if(packetSendWrapper.packet.packetType == EPacketType.None){
            throw new NoCommandCodeInPacketException("A Packet doesn't have its own packet type!");
        }
        SendPacketServerRpc(packetSendWrapper, new ServerRpcParams());
    }


    // [ServerRpc(RequireOwnership = false)]
    // private void SendPacketToAllServerRpc(Packet packet, ServerRpcParams serverRpcParams){
    //     ReceivePacketClientRpc(packet, new ClientRpcParams());
    // }

    [ServerRpc(RequireOwnership = false)]
    private void SendPacketServerRpc(PacketSendWrapper packetSendWrapper, ServerRpcParams serverRpcParams){
        if(!IsServer){ // 혹시 모를 예외 방지 (서버에서만 실행되게, 근데 어차피 서버에서만 될거임)
            return;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams();
        if(packetSendWrapper.networkTargets != null){
            clientRpcParams.Send = new ClientRpcSendParams{
                TargetClientIds = packetSendWrapper.GetClientIds()
            };
        }

        ReceivePacketClientRpc(packetSendWrapper.packet, clientRpcParams);

        // if(packetSendWrapper.networkTargets != null && packetSendWrapper.HasNonClient()){
        //     ulong[] networkObjectIds = packetSendWrapper.GetNonClientNetworkObjectIds();

        // }
    }

    [ClientRpc]
    private void ReceivePacketClientRpc(Packet packet, ClientRpcParams clientRpcParams){
        Debug.Log($"Receive '{packet}', by " + ClientManager.Instance.ClientInfo.clientId);

        PacketReceiveHandler.DecodePacket(packet);
    }

}

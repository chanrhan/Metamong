using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerPacketReceiveHandler : PacketHandler
{
    public static void DecodePacket(PacketSendWrapper packetSendWrapper){
        switch(packetSendWrapper.packet.packetType){
            case EPacketType.Talk:
                Chat(packetSendWrapper);
                break;
        }
    }

    public static void TalkByNPC(string msg, NetworkTarget[] targets){
        Debug.Log("Talk By NPC + " + msg);
        if(msg.Length > MAX_MESSAGE_LENGTH){
            msg = msg.Substring(0, MAX_MESSAGE_LENGTH);
        }

        PacketSendWrapper packetSendWrapper = new PacketSendWrapper{
            networkTargets = targets,
            packet = new Packet{
                packetType = EPacketType.Talk,
                message = msg,
                senderId = 1000,
                clientInfo = new ClientInfo{
                    clientId = 1000,
                    username = "NPC_TEST",
                    isHost = false
                }
            }
        };
        Chat(packetSendWrapper);
    }


    private static void Chat(PacketSendWrapper packetSendWrapper){
        ClientRpcParams clientRpcParams = new ClientRpcParams();
        if(packetSendWrapper.networkTargets != null){
            clientRpcParams.Send = new ClientRpcSendParams{
                TargetClientIds = packetSendWrapper.GetClientIds()
            };
        }

        RpcManager.Instance.ReceivePacketClientRpc(packetSendWrapper.packet, clientRpcParams);

        // 클라이언트가 아닌 대상(ex. NPC) 에게 보내는 로직 
        if(packetSendWrapper.networkTargets != null && packetSendWrapper.HasNonClient()){
            ulong[] networkObjectIds = packetSendWrapper.GetNonClientNetworkObjectIds();
            string msg = packetSendWrapper.packet.message;
            ulong senderClientId = packetSendWrapper.packet.senderId;
            NetworkObject senderObject = CustomNetworkManager.Instance.GetNetworkObjectByClientId(senderClientId);

            foreach(ulong id in networkObjectIds){
                if(CustomNetworkManager.Instance.TryGetNetworkObjectById(id, out NetworkObject networkObject)){
                    if(networkObject.TryGetComponent(out IListenable listenable)){
                        listenable.ListenMessage(senderObject.gameObject, msg);
                    }
                }
            }
        }
    }
}

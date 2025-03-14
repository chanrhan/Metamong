using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 서버에서 패킷을 수신하는 핸들러
/// </summary>
public class ServerPacketReceiveHandler : PacketHandler
{
    /// <summary>
    /// 패킷을 디코딩하는 함수
    /// </summary>
    /// <param name="packetSendWrapper">디코딩할 PacketSendWrapper</param>
    public static void DecodePacket(PacketSendWrapper packetSendWrapper){
        switch(packetSendWrapper.packet.packetType){
            case EPacketType.Talk:
                Chat(packetSendWrapper);
                break;
        }
    }

    /// <summary>
    /// NPC가 채팅 메세지를 보내는 함수
    /// </summary>
    /// <param name="msg">보낼 메세지</param>
    /// <param name="targets">수신할 네트워크 대상 객체들</param>
    public static void TalkByNPC(string msg, NetworkTarget[] targets){
        if(msg.Length > MAX_MESSAGE_LENGTH){
            msg = msg.Substring(0, MAX_MESSAGE_LENGTH);
        }

        PacketSendWrapper packetSendWrapper = new PacketSendWrapper{
            networkTargets = targets,
            packet = new Packet{
                packetType = EPacketType.Talk,
                message = msg,
                senderId = 1000,

                // NPC는 클라이언트 정보가 없으므로, 임의로 만들어 주었음
                clientInfo = new ClientInfo{
                    clientId = 1000,
                    username = "NPC_TEST",
                    isHost = false
                }
            }
        };
        Chat(packetSendWrapper);
    }


    /// <summary>
    /// 채팅 메세지를 클라이언트로 보내는 함수
    /// </summary>
    private static void Chat(PacketSendWrapper packetSendWrapper){
        ClientRpcParams clientRpcParams = new ClientRpcParams();

        // 수신한 클라이언트들을 설정
        if(packetSendWrapper.networkTargets != null){
            clientRpcParams.Send = new ClientRpcSendParams{
                TargetClientIds = packetSendWrapper.GetClientIds()
            };
        }
        Debug.Log("Chat To " + packetSendWrapper.networkTargets[0].networkObjectId);

        // 클라이언트에게 메세지 전송
        RpcManager.Instance.ReceivePacketClientRpc(packetSendWrapper.packet, clientRpcParams);

        // 클라이언트가 아닌 대상(ex. NPC) 에게 보내는 로직 
        if(packetSendWrapper.networkTargets != null && packetSendWrapper.HasNonClient()){
            ulong senderClientId = packetSendWrapper.packet.senderId;

            Debug.Log(111);
            if(CustomNetworkManager.Instance.TryGetNetworkObjectByClientId(senderClientId, out NetworkObject senderObject)){
                Debug.Log(222);
                // 클라이언트가 아닌 네트워크 대상 객체의 ID 들을 가져옴
                ulong[] networkObjectIds = packetSendWrapper.GetNonClientNetworkObjectIds();
                string msg = packetSendWrapper.packet.message;

                foreach(ulong id in networkObjectIds){
                    if(CustomNetworkManager.Instance.TryGetNetworkObjectById(id, out NetworkObject networkObject)){
                        // IListenable 을 가지고 있는 객체들에게 모두 메세지 전송
                        if(networkObject.TryGetComponent(out IListenable listenable)){
                            listenable.ListenMessage(senderObject.gameObject, msg);
                        }
                    }
                }
            }
            
            
        }
    }
}

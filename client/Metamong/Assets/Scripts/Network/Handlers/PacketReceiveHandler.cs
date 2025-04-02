using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 클라이언트에서 패킷을 수신하는 핸들러
/// </summary>
public class PacketReceiveHandler : PacketHandler
{
    /// <summary>
    /// 패킷을 디코딩하는 함수
    /// </summary>
    /// <param name="packet">디코딩할 패킷</param>
    public static void DecodePacket(Packet packet){
        switch(packet.packetType){
            case EPacketType.Talk:
                ReceiveChatMessage(packet);
                break;
        }
    }

    /// <summary>
    /// 채팅 메세지를 수신하는 함수
    /// </summary>
    /// <param name="packet"></param>
    private static void ReceiveChatMessage(Packet packet){
        ClientInfo clientInfo = packet.clientInfo;
        LlmManager.Instance.AddChatLog(clientInfo.username, packet.message);
        // 채팅창에 띄우기

        Debug.Log("Receive Input Chat");
        ChatManager.Instance.InputChat(clientInfo.username, packet.message);
    }
}

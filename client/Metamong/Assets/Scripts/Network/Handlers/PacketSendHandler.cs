using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 클라이언트에서 패킷을 전송하는 핸들러
/// </summary>
public class PacketSendHandler : PacketHandler
{
    /// <summary>
    /// 채팅 메세지를 전송하는 함수
    /// </summary>
    /// <param name="msg">보낼 메세지</param>
    /// <param name="targets">수신할 네트워크 객체들 (빈 값일 경우 클라이언트 모두에게 전송)</param>
    public static void ChatText(string msg, NetworkTarget[] targets = null){
        Chat(ClientManager.Instance.ClientInfo, msg, targets);
    }


    private static void Chat(ClientInfo clientInfo, string msg, NetworkTarget[] targets = null){
        // 전송되는 메세지의 최대 길이를 제한, 버퍼 초과 오류가 발생할 가능성이 있다고 함 
        if(msg.Length > MAX_MESSAGE_LENGTH){
            msg = msg.Substring(0, MAX_MESSAGE_LENGTH);
        }
        
        Packet packet = new Packet{
            packetType = EPacketType.Talk,
            message = msg,
            clientInfo = clientInfo
        };
        RpcManager.Instance.SendPacketTo(packet, targets);

    }
}

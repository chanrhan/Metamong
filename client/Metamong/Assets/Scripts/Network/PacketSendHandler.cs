using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PacketSendHandler : PacketHandler
{
    public static void Talk(string msg, NetworkTarget[] targets){
        Talk(ClientManager.Instance.ClientInfo, msg, targets);
    }

    private static void Talk(ClientInfo clientInfo, string msg, NetworkTarget[] targets){
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

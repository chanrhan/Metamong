using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PacketSendHandler 
{
    public static void Talk(string msg, NetworkTarget[] targets){
        Talk(ClientManager.Instance.ClientInfo, msg, targets);
    }

    public static void TalkByNPC(string msg, NetworkTarget[] targets){
        ClientInfo npcInfo = new ClientInfo{
            clientId = 1000,
            username = "NPC_TEST",
            isHost = false
        };
        Talk(npcInfo, msg, targets);
    }

    private static void Talk(ClientInfo clientInfo, string msg, NetworkTarget[] targets){
        Packet packet = new Packet{
            packetType = EPacketType.Talk,
            msg = msg,
            clientInfo = clientInfo
        };
        RpcManager.Instance.SendPacketTo(packet, targets);

    }
}

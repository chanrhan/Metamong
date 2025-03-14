using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketReceiveHandler 
{
    public static void DecodePacket(Packet packet){
        switch(packet.packetType){
            case EPacketType.Talk:
                ReceiveMessage(packet);
                break;
        }
    }

    private static void ReceiveMessage(Packet packet){
        ClientInfo clientInfo = packet.clientInfo;
        ChatManager.Instance.InputChat(clientInfo.username, packet.msg);
    }
}

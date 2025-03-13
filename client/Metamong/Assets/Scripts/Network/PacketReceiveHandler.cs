using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketReceiveHandler 
{
    public static void DecodePacket(Packet packet){
        switch(packet.commandCode){
            case ECommandCode.Talk:
                Talk(packet);
                break;
        }
    }

    private static void Talk(Packet packet){
        ClientInfo clientInfo = packet.clientInfo;
        ChatManager.Instance.InputChat(clientInfo.username, packet.msg);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketSendHandler 
{
    public static void Talk(string msg, ulong[] targetIds = default){
        Packet packet = new Packet{
            commandCode = EPacketType.Talk,
            msg = msg,
            clientInfo = ClientManager.Instance.ClientInfo
        };

        RpcManager.Instance.SendPacketTo(packet, targetIds);
    }
}

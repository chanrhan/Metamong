using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// RPC (Remote Procedure Call) 원격 프로시저 호출 매니저.
/// Netcode 통신 관련해서 종단 기능을 담당한다.
/// </summary>
public class RpcManager : NetworkBehaviourSingleton<RpcManager>
{
    // public static RpcManager Instance { get; private set; }

    // private void Awake() {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }

    /// <summary>
    /// 클라이언트(또는 호스트)에서 서버로 패킷을 보내는 함수
    /// </summary>
    /// <param name="packet">보낼 데이터</param>
    /// <param name="targets">수신받을 네트워크 객체들</param>
    public void SendPacketTo(Packet packet, NetworkTarget[] targets = null){
        PacketSendWrapper sendWrapper = new PacketSendWrapper{
            packet = packet,
            networkTargets = targets
        };
        SendPacket(sendWrapper);
    }

    private void SendPacket(PacketSendWrapper packetSendWrapper){
        Debug.Log("Send Packet");
        if(packetSendWrapper.packet.packetType == EPacketType.None){
            throw new NoCommandCodeInPacketException("A Packet doesn't have its own packet type!");
        }
        SendPacketServerRpc(packetSendWrapper, new ServerRpcParams());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendPacketServerRpc(PacketSendWrapper packetSendWrapper, ServerRpcParams serverRpcParams){
        if(!IsServer){ // 혹시 모를 예외 방지 (서버에서만 실행되게, 근데 어차피 서버에서만 될거임)
            return;
        }

        ServerPacketReceiveHandler.DecodePacket(packetSendWrapper);
    }

    /// <summary>
    /// 서버에서 클라이언트로 패킷을 전송하는 함수
    /// </summary>
    /// <param name="packet"></param>
    [ClientRpc]
    public void ReceivePacketClientRpc(Packet packet, ClientRpcParams clientRpcParams = default){
        Debug.Log("Receive Packet");
        PacketReceiveHandler.DecodePacket(packet);
    }

}

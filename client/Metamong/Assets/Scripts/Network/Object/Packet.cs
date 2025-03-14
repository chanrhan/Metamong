using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 통신 과정애서 쓰이는 데이터 전송 객체 (Data Transfer Object, DTO)
/// </summary>
public struct Packet : INetworkSerializable
{
    public EPacketType packetType;
    public ulong senderId;

    // 보낼 메세지
    public string message;

    // 송신 클라이언트 정보
    public ClientInfo clientInfo;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref packetType);
        serializer.SerializeValue(ref senderId);
        // serializer.SerializeValue(ref targetIds);
        serializer.SerializeValue(ref message);
        serializer.SerializeValue(ref clientInfo);
    }

}

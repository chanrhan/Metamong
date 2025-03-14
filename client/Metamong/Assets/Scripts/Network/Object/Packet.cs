using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct Packet : INetworkSerializable
{
    public EPacketType packetType;
    public ulong senderId;
    // public ulong[] targetIds;
    public string message;
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

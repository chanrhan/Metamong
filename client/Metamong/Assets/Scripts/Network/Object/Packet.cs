using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct Packet : INetworkSerializable
{
    public EPacketType commandCode;
    public ulong senderId;
    // public ulong[] targetIds;
    public string msg;
    public ClientInfo clientInfo;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref commandCode);
        serializer.SerializeValue(ref senderId);
        // serializer.SerializeValue(ref targetIds);
        serializer.SerializeValue(ref msg);
        serializer.SerializeValue(ref clientInfo);
    }

}

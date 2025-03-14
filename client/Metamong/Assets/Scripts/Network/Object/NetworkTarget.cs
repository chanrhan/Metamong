using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTarget : INetworkSerializable
{
    public ulong networkObjectId;
    public ulong clientId;
    public bool isClient = true;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref networkObjectId);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref isClient);
    }
}

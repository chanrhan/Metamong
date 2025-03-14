using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 특정 클라이언트로의 패킷 전송을 위해 한번 더 캡슐화한 객체 
/// </summary>
public struct PacketSendWrapper : INetworkSerializable
{
    public NetworkTarget[] networkTargets;
    public Packet packet;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref packet);
        serializer.SerializeValue(ref networkTargets);
    }

    public ulong[] GetClientIds(){
        return networkTargets.Where(target=>target.isClient).Select(target=>target.clientId).ToArray();
    }

    public bool HasNonClient(){
        return networkTargets.Any(target=>!target.isClient);
    }

    public ulong[] GetNonClientNetworkObjectIds(){
        return networkTargets.Where(target=>!target.isClient).Select(target=>target.networkObjectId).ToArray();
    }
}
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 네트워크 객체 전송에 대하여 필요한 정보만 모아 놓은 객체
/// NPC, 또는 플레이어에서 해당 NetworkTarget를 추출한 후 전송하면 해당 대상에게 전송된다.
/// </summary>
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

    public override string ToString(){
        return "clientId: " + clientId + ", networkObjectId: " + networkObjectId;
    }
}

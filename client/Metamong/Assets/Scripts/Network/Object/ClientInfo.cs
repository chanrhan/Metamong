using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 클라이언트 정보
/// </summary>
[Serializable]
public struct ClientInfo : INetworkSerializable
{
    public ulong clientId;
    public string username;
    public bool isHost;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref username);
        serializer.SerializeValue(ref username);
    }

    public override string ToString(){
        return $"<clientId: {clientId}, username: {username}, isHost: {isHost}>";
    }
}

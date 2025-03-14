using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패킷의 타입.
/// 해당 패킷의 목적에 따라 PacketHandler에서 이를 나눠서 처리한다.
/// </summary>
public enum EPacketType 
{
    None,
    Instantiate,
    Destroy,
    Talk,
    Speak,
    Sound,
    Animation,
    Disconnect
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 객체에 대한 C# Extension 을 설정하는 클래스.
/// C# Extension이 뭐냐고? 윤민이한테 물어봐
/// </summary>
public static class ObjectExtensions 
{
    public static NetworkTarget ToNetworkTarget(this NetworkObject networkObject){
        return new NetworkTarget{
            clientId = networkObject.OwnerClientId,
            networkObjectId = networkObject.NetworkObjectId,
            isClient = networkObject.TryGetComponent(out PlayerController p)
        };
    }
}

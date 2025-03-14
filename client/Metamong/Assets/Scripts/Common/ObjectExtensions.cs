using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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

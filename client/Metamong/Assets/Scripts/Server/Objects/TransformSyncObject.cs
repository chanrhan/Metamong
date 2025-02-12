using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TransformSyncObject : NetworkBehaviour
{
    public NetworkVariable<Vector3> NetPosition = new NetworkVariable<Vector3>(
        Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private void Update() {
        if(IsHost || IsClient){
            transform.position = NetPosition.Value;
        }
    }
}

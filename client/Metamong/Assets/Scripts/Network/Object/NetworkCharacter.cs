using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class NetworkCharacter : NetworkBehaviour, IListenable
{
    //대화 관련
    public float speekRange = 5.0f;       // 채팅 전송 범위
    
    protected bool TryGetAroundNetworkTargets(out NetworkTarget[] networkTargets){
        RaycastHit[] hitPlayers = Physics.SphereCastAll(transform.position, speekRange, Vector3.up, 0.0f, 64); //64 = Conversable Layer(2^7)
        
        List<NetworkTarget> targets = new List<NetworkTarget>();
        foreach (RaycastHit hit in hitPlayers)
        {
            if (hit.transform.CompareTag("Player") && hit.transform.TryGetComponent(out IListenable i))
            {
                NetworkObject no = hit.transform.GetComponent<NetworkObject>();
                if(no){
                    targets.Add(no.ToNetworkTarget());
                }
            }
        }
        networkTargets = targets.ToArray();
        return targets.Count > 0;
    }
    public abstract void ListenMessage(GameObject senderObj, string message);
    public abstract void SendMessageToOthers();

}

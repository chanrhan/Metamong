using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class NetworkCharacter : NetworkBehaviour, IListenable
{
    //대화 관련
    public float speekRange = 5.0f;       // 채팅 전송 범위
    protected AvatarAnimation avatarAnim;

    protected Rigidbody myRigid;
    protected Collider myCollider;

    public bool IsAnimPlaying
    {
        get => avatarAnim.IsMotionPlaying;
        set => avatarAnim.IsMotionPlaying = value;
    }
    public bool IsTalking
    {
        get => avatarAnim.IsTalking;
        //set => avatarAnim.IsTalking = value;
        set
        {
            avatarAnim.IsTalking = value;
            avatarAnim.TalkingPara = value ? Random.Range(0, 3) : -1;
        }
    }

    protected virtual void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();
        avatarAnim = GetComponent<AvatarAnimation>();
    }

    protected bool TryGetAroundNetworkTargets(string targetTag, out NetworkTarget[] networkTargets)
    {
        RaycastHit[] hitPlayers = Physics.SphereCastAll(transform.position, speekRange, Vector3.up, 0.0f, 64); //64 = Conversable Layer(2^7)

        List<NetworkTarget> targets = new List<NetworkTarget>();
        foreach (RaycastHit hit in hitPlayers)
        {
            if ((targetTag == null || hit.transform.tag == targetTag) && hit.transform.TryGetComponent(out IListenable i))
            {
                NetworkObject no = hit.transform.GetComponent<NetworkObject>();
                if (no)
                {
                    NetworkTarget nt = no.ToNetworkTarget();

                    // 자기 자신에게는 보내지 않음 (메아리 X)
                    if (NetworkObjectId != nt.networkObjectId)
                    {
                        targets.Add(nt);
                    }
                }
            }
        }
        networkTargets = targets.ToArray();
        return targets.Count > 0;
    }

    protected bool TryGetAroundPlayers(out NetworkTarget[] networkTargets)
    {
        return TryGetAroundNetworkTargets("Player", out networkTargets);
    }

    protected bool TryGetAroundNPC(out NetworkTarget[] networkTargets)
    {
        return TryGetAroundNetworkTargets("OtherPlayer", out networkTargets);
    }

    protected bool TryGetAroundAll(out NetworkTarget[] networkTargets)
    {
        return TryGetAroundNetworkTargets(null, out networkTargets);
    }

    public abstract void ListenMessage(GameObject senderObj, string message);
    public abstract void SendMessageToOthers(string text);
    public abstract void PlayMotion(string faceClipName, string actionClipName);

}

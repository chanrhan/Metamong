using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class PlayerDetection : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.npc.detectedPlayer == null) return State.Failure;
        else
        {
            RaycastHit[] hits = Physics.SphereCastAll(context.transform.position, 2.0f, Vector3.up, 0.0f, 64);
            for (int idx = 0; idx < hits.Length; idx++)
            {
                if (hits[idx].transform.gameObject == context.npc.detectedPlayer)
                {
                    Debug.Log("플레이어 근처에 있음");
                    context.animator.SetBool("isWalking", false);
                    return State.Success;
                }
            }
            context.npc.EndConversation();
            return State.Failure;
        }
    }
}

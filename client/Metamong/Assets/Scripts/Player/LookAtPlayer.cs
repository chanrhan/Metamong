using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class LookAtPlayer : ActionNode
{
    protected override void OnStart() {

    }

    protected override void OnStop() {

    }

    protected override State OnUpdate() {
        Quaternion dest 
            = Quaternion.LookRotation(new Vector3(
                context.npc.detectedPlayer.transform.position.x - context.transform.position.x,
                0.0f,
                context.npc.detectedPlayer.transform.position.z - context.transform.position.z)
            );

        while (Quaternion.Angle(context.transform.rotation, dest) > 0.05f)
        {
            context.transform.rotation = Quaternion.Lerp(context.transform.rotation, dest, 0.1f);
            return State.Running;
        }
        return State.Success;
    }
}

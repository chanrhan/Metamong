using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class RandomAnimation : ActionNode
{
    protected override void OnStart() {
        Debug.Log("RandomAnimation");
    }

    protected override void OnStop() {
        
        Debug.Log("Animation Stopped");
    }

    protected override State OnUpdate() {
        return State.Success;
    }
}

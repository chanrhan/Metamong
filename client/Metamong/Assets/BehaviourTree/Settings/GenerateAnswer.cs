using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class GenerateAnswer : ActionNode
{
    protected override void OnStart() {
        
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.npc.isGeneratingAnswer)
        {
            return State.Failure;
        }
        else
        {
            return State.Success;
        }
    }
}

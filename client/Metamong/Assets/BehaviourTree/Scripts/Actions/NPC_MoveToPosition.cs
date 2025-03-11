using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class NPC_MoveToPosition : ActionNode
{
    public float speed = 5;
    public float stoppingDistance = 0.1f;
    public bool updateRotation = true;
    public float acceleration = 40.0f;
    public float tolerance = 1.0f;

    protected override void OnStart()
    {
        context.agent.stoppingDistance = stoppingDistance;
        context.agent.speed = speed;
        context.agent.destination = blackboard.moveToPosition;
        context.agent.updateRotation = updateRotation;
        context.agent.acceleration = acceleration;

        context.animator.SetTrigger("StopTrigger");
        //context.transform.GetComponent<NpcAnimationSelector>().isPlaying = false;
        context.animator.SetBool("isWalking", true);
    }

    protected override void OnStop()
    {
        context.animator.SetBool("isWalking", false);
    }

    protected override State OnUpdate()
    {
        if (context.agent.pathPending)
        {
            return State.Running;
        }

        if (context.agent.remainingDistance < tolerance)
        {
            context.animator.SetBool("isWalking", false);
            return State.Success;
        }

        if (context.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathInvalid)
        {
            context.animator.SetBool("isWalking", false);
            return State.Failure;
        }

        return State.Running;
    }
}

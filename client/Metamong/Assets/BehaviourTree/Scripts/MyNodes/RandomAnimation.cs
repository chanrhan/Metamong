using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class RandomAnimation : ActionNode
{
    protected override void OnStart() {
        //NpcAnimationSelector selector = context.transform.GetComponent<NpcAnimationSelector>();
       // if (selector.isPlaying) return;

      //  string targetClipName = selector.GetRandomAnimationName();
       // selector.isPlaying = true;
        //context.animator.Play(targetClipName,0);
    }

    protected override void OnStop() {
        //NpcAnimationSelector selector = context.transform.GetComponent<NpcAnimationSelector>();
        context.animator.SetTrigger("StopTrigger");
        //selector.isPlaying = false;
    }

    protected override State OnUpdate() {
        if (context.npc.detectedPlayer != null) {
            Debug.Log("랜덤 애니메이션 실패");
            return State.Success;
        }
        if (context.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.95f)
        {
            Debug.Log("랜덤 애니메이션 실행중");
            return State.Running;
        }
        //NpcAnimationSelector selector = context.transform.GetComponent<NpcAnimationSelector>();
        context.animator.SetTrigger("StopTrigger");
       // selector.isPlaying = false;
        return State.Success;
    }
}
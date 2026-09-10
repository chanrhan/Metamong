using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NpcAnimationController : MonoBehaviour
{
    private Animator myAnimator;
    private NavMeshAgent myAgent;
    [SerializeField]
    private List<string> dailyAnimList = new List<string>();
    public Animator MyAnimator { get => myAnimator; }
    public NavMeshAgent MyAgent { get => myAgent; }

    private void Awake()
    {
        myAnimator = GetComponentInChildren<Animator>();
        myAgent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 이동 관련된 부분들을 모두 해제하는 메서드. 애니메이션, navMeshAgent의 이동을 모두 해제함.
    /// </summary>
    public void StopAllMovement()
    {
        myAgent.ResetPath();
        myAnimator.SetBool("isWalking", false);
        //myAnimator.StopPlayback();
        myAnimator.SetTrigger("StopTrigger");
    }

    /// <summary>
    /// 현재 등록된 랜덤 애니메이션을 재생하는 메서드
    /// </summary>
    /// <param name="layer">원하는 애니메이션 레이어</param>
    public void PlayRandomAnimation(int layer)
    {
        myAnimator.Play(dailyAnimList[Random.Range(0, dailyAnimList.Count)], 0);
    }

    public void PlayAnimation(Text name){
        myAnimator.Play(name.text, 0);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 또는 NPC의 애니메이션 제어를 담당하는 클래스 
/// </summary>
public class AvatarAnimation : MonoBehaviour
{
    [SerializeField]
    private string idleClipName = "IdleAnimation"; // Idle 애니메이션 클립명 
    [SerializeField]
    private string talkingClipName = "";

    private const string TALKING_PARAM = "isTalking";
    private const string WALKING_PARAM = "isWalking";
    private const string TALKING_VAR_PARA = "TalkingPara";

    private Animator anim;
    [SerializeField]
    private bool isBlocked = false; // 애니메이션 입력 방지 변수, Block인 경우에는 애니메이션 인터럽트가 발생하지 않는다 
    [SerializeField]
    private bool isMotionPlaying = false; // 모션 실행 중 여부 체크
    public bool IsBlocked
    {
        get => isBlocked;
        set
        {
            isBlocked = value;
            if (!value)
            {
                isMotionPlaying = false;
            }
        }
    }
    public bool IsMotionPlaying
    {
        get => isMotionPlaying;
        set => isMotionPlaying = value;
    }
    public bool IsTalking
    {
        get=>anim.GetBool(TALKING_PARAM);
        set=>anim.SetBool(TALKING_PARAM, value);
    }

    public int TalkingPara
    {
        get=>anim.GetInteger(TALKING_VAR_PARA);
        set => anim.SetFloat(TALKING_VAR_PARA, value);
    }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
        {
            Debug.LogError("Animator component not found in children.");
        }
    }

    void Update()
    {
        // Debug.Log($"[chan] ({transform.name}) Blocked: {isBlocked}, IsMotionPlaying: {IsMotionPlaying}");
        UpdateIdleTransition();
    }


    private bool IsDefaultAnimBool()
    {
        return anim.GetBool(WALKING_PARAM);
    }

    /// <summary>
    /// 특정 애니메이션이 끝난 후, 애니메이션 상태를 강제로 Idle로 바꿔버리는 함수
    /// isWalking 등의 trigger로 Animation Transition을 제어하면, has Exit Time을 사용할 수 없기 때문에,
    /// 코드로 제어하기로 하였다. 
    /// </summary>
    private void UpdateIdleTransition()
    {
        if (isBlocked)
        {
            if (IsDefaultAnimBool())
            {
                isBlocked = false;
                isMotionPlaying = false;
                IsTalking = false;
                return;
            }

            if (isMotionPlaying)
            {
                AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);

                // 트리거가 있는 애니메이션 변수가 True가 되거나, 애니메이션 실행이 거의 완료되었을 경우, Idle로 복귀 
                if (IsDefaultAnimBool() || state.normalizedTime >= 0.95f)
                {
                    Debug.Log("[chan] Idle");
                    anim.Play(idleClipName);
                    isBlocked = false;
                    isMotionPlaying = false;
                    IsTalking = false;
                }
            }

        }

    }


    public void StopWalking()
    {
        anim.SetBool(WALKING_PARAM, false);
    }
    public void StartWalking()
    {
        anim.SetBool(WALKING_PARAM, true);
    }

    public void StartTalking()
    {
        anim.SetBool(TALKING_PARAM, true);
    }
    public void StopTalking()
    {
        anim.SetBool(TALKING_PARAM, false);
    }

    public void PlayFaceAndActionAnimation(string faceClipName, string actionClipName)
    {
        Debug.Log($"[chan] Play : {actionClipName}, Face: {faceClipName}");
        isBlocked = true;
        isMotionPlaying = true;
        IsTalking = false;
        anim.Play(actionClipName, 0);
        anim.Play(faceClipName, 2);
    }

    public void StartJump()
    {
        anim.Play("Jump");
    }
}

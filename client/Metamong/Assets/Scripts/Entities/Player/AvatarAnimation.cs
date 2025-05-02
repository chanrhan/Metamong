using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimation : MonoBehaviour
{
    [SerializeField]
    private string idleClipName = "IdleAnimation";

    private Animator anim;
    private bool isBlocked = false;
    private bool isMotionPlaying = false;
    public bool IsBlocked
    {
        get => isBlocked;
        set => isBlocked = value;
    }
    public bool IsMotionPlaying
    {
        get => isMotionPlaying;
        set => isMotionPlaying = value;
    }

    

    private string currentPlayingClipName = string.Empty;

    public bool IsPlayingIdleAnim
    {
        get => anim.GetCurrentAnimatorStateInfo(0).IsName(idleClipName);
    }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if(anim == null)
        {
            Debug.LogError("Animator component not found in children.");
        }
    }

    void Update()
    {
        Debug.Log($"[chan] Blocked: {isBlocked}, IsMotionPlaying: {IsMotionPlaying}");
        UpdateIdleTransition();
    }


    private bool IsDefaultAnimBool()
    {
        return anim.GetBool("isWalking") || anim.GetBool("isTalking");
    }

    private void UpdateIdleTransition()
    {
        if (isBlocked && IsMotionPlaying)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            // Debug.Log($"[chan] Blocked: {state.normalizedTime}");

            if (IsDefaultAnimBool() || state.normalizedTime >= 0.9f)
            {
                Debug.Log("[chan] Idle");
                anim.Play(idleClipName);
                isBlocked = false;
                isMotionPlaying = false;
            }
        }
        
    }


    public void StopWalking()
    {
        anim.SetBool("isWalking", false);
    }
    public void StartWalking()
    {
        anim.SetBool("isWalking", true);
    }

    public void StartTalking()
    {
        anim.SetBool("isTalking", true);
    }
    public void StopTalking()
    {
        anim.SetBool("isTalking", false);
    }

    public void PlayFaceAndActionAnimation(string faceClipName, string actionClipName)
    {
        Debug.Log($"[chan] Play : {actionClipName}, Face: {faceClipName}");
        isBlocked = true;
        isMotionPlaying = true;
        anim.Play(faceClipName, 0);
        anim.Play(actionClipName, 2);
    }
}

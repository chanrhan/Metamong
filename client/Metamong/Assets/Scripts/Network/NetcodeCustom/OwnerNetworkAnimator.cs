using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// 클라이언트도 해당 Animation 을 제어할 수 있도록 기존 NetworkAnimator을 상속한 객체
/// </summary>
public class OwnerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}

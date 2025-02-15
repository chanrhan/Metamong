using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity;
using Unity.Netcode.Components;

/// <summary>
/// 클라이언트도 해당 Transform 을 제어할 수 있도록 기존 NetworkTransform을 상속한 객체
/// </summary>
public class ClientNetworkTransform : NetworkTransform
{
    /// <summary>
    /// 서버 권한 해제
    /// </summary>
    /// <returns></returns>
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

}
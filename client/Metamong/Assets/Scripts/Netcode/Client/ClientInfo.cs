using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 클라이언트의 정보를 저장하는 클래스
/// </summary>
[System.Serializable]
public class ClientInfo
{
    public ulong clientId;
    public string username;
    public bool isHost;
}
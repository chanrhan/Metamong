using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class ClientInfo
{
    public ulong clientId;
    public string username;
    public bool isHost;
    public GameObject playerPrefab;
}
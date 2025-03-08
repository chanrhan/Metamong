using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// 전체 네트워크 통신 관리자
/// </summary>
public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance { get; private set; }

    [SerializeField]
    private NetworkVariable<Dictionary<ulong, ClientInfo>> userList = new NetworkVariable<Dictionary<ulong, ClientInfo>>(
        new Dictionary<ulong, ClientInfo>(),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake() {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            OnClientConnectedCallback += OnClientJoined;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 클라이언트 접속 시, 콜백되는 함수 
    /// 클라이언트에 ClientId를 갱신하고, 서버에 자신의 ClinetInfo 등록 
    /// </summary>
    /// <param name="clientId"></param>
    private void OnClientJoined(ulong clientId){
        Debug.Log($"Client [{clientId}] Joined!");

        ClientInfo myPlayerInfo = ClientManager.Instance.ClientInfo;
        myPlayerInfo.clientId = clientId;

        userList.Value.Add(clientId, myPlayerInfo);
    }

    public void SetPort(ushort port){
        GetComponent<UnityTransport>().ConnectionData.Port = port;
    }

    public void JoinHost()
    {
        Debug.Log($"Welcome {ClientManager.Instance.ClientInfo.username}");
        StartHost();
    }

    public void JoinClient()
    {
        Debug.Log($"Welcome {ClientManager.Instance.ClientInfo.username}");
        StartClient();
    }
}

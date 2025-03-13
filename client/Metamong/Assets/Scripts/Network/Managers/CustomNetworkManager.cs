using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance { get; private set; }

    [SerializeField]
    private Dictionary<ulong, ClientInfo> userList = new Dictionary<ulong, ClientInfo>();

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

    public NetworkObject GetNetworkObjectByClientId(ulong id){
        if(ConnectedClients.TryGetValue(id, out var client)){
            return client.PlayerObject;
        }
        return null;
    }

    public bool GetClientInfo(ulong id, out ClientInfo clientInfo){
        return userList.TryGetValue(id, out clientInfo);
    }
}

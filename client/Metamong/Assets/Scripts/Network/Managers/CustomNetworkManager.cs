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
            OnClientDisconnectCallback += OnClientDisconnected;
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
        Debug.Log($"Client [{clientId}] Connected!");
    }

    private void OnClientDisconnected(ulong clientId){
        Debug.Log($"Client [{clientId}] Disconnected!");
    }


    public void SetUnityTransport(string ipAddress, ushort port){
        UnityTransport unityTransport = GetComponent<UnityTransport>();
        unityTransport.ConnectionData.Address = ipAddress;
        unityTransport.ConnectionData.Port = port;
    }

    public void JoinHost()
    {
        Debug.Log($"Welcome Host {ClientManager.Instance.ClientInfo.username}");
        StartHost();
    }

    public void JoinClient()
    {
        Debug.Log($"Welcome Client {ClientManager.Instance.ClientInfo.username}");
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

    public void Disconnect(){
        Shutdown();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Linq;

/// <summary>
/// Netcode 통신의 중추.
/// </summary>
public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance { get; private set; }

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

    /// <summary>
    /// 접속할 IP 주소와 Port를 설정하는 함수
    /// </summary>
    /// <param name="ipAddress">접속한 IP 주소</param>
    /// <param name="port">접속할 Port</param>
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

    /// <summary>
    /// Netcode에서 부여한 클라이언트 Id로 해당 NetworkObject를 찾는 함수
    /// </summary>
    /// <param name="clientId">찾기 위해 사용할 클라이언트 ID</param>
    /// <param name="networkObject">탐색 성공 시 반환될 NetworkObject</param>
    /// <returns></returns>
    public bool TryGetNetworkObjectByClientId(ulong clientId, out NetworkObject networkObject){
        if(ConnectedClients.TryGetValue(clientId, out var client)){
            networkObject = client.PlayerObject;
            return true;
        }
        networkObject = null;
        return false;
    }

    /// <summary>
    /// Netcode에서 부여한 NetworkObject ID로 해당 NetworkObject를 찾는 함수
    /// </summary>
    /// <param name="networkObjectId">찾기 위해 사용할 NetworkObject ID</param>
    /// <param name="networkObject">탐색 성공 시 반환될 NetworkObject</param>
    /// <returns></returns>
    public bool TryGetNetworkObjectById(ulong networkObjectId, out NetworkObject networkObject){
        return SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out networkObject);
    }


    public void Disconnect(){
        Shutdown();
    }
}

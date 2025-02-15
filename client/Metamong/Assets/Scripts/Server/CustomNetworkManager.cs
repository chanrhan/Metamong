using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

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
            // NetworkConfig.ConnectionApproval = true;
            // NetworkConfig.PlayerPrefab = null;

            OnClientConnectedCallback += OnClientJoined;
            // ConnectionApprovalCallback = ApprovalCheck;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnClientJoined(ulong clientId){
        Debug.Log($"Client [{clientId}] Joined!");

        ClientInfo myPlayerInfo = ClientManager.Instance.ClientInfo;
        myPlayerInfo.clientId = clientId;
        // myPlayerInfo.playerPrefab = SpawnManager.GetLocalPlayerObject().gameObject;
        userList.Value.Add(clientId, myPlayerInfo);

        // OnClientConnectedCallback -= OnClientJoined;
    }

    // private void ApprovalCheck(ConnectionApprovalRequest request, ConnectionApprovalResponse response){
    //     Debug.Log("Approval Check");
    //     response.Approved = true;
    //     response.CreatePlayerObject = false; // 넷코드의 자동 생성 비활성화
    //     response.Pending = false; // 승인 완료

    //     GameObject playerPrefab = ClientManager.Instance.MyPlayerInfo.playerPrefab;
        
    //     // 직접 PlayerPrefab을 인스턴스화
    //     GameObject playerObject = Instantiate(playerPrefab);
    //     Debug.Log($"Local: {LocalClientId}, req: {request.ClientNetworkId}");
    //     if(LocalClientId == request.ClientNetworkId){
    //         playerObject.AddComponent<PlayerInputController>(); // 원하는 컴포넌트 추가
    //     }

    //     // NetworkObject를 Spawn하면서 해당 클라이언트를 소유자로 설정
    //     NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
    //     Debug.Log($"Client Id: {request.ClientNetworkId}");
    //     networkObject.SpawnWithOwnership(request.ClientNetworkId);
    // }

    public void SetPort(ushort port){
        GetComponent<UnityTransport>().ConnectionData.Port = port;
    }

    public GameObject GetPlayerObject(ulong clientId){
        return userList.Value[clientId].playerPrefab;
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

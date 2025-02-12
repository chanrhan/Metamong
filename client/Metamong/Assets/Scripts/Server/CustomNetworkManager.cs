using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net.NetworkInformation;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance { get; private set; }

    [SerializeField]
    private NetworkVariable<Dictionary<ulong, MyPlayerInfo>> userList = new NetworkVariable<Dictionary<ulong, MyPlayerInfo>>(
        null,
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

    private void OnClientJoined(ulong clientId){
        Debug.Log($"Client {clientId} Joined!");
        Debug.Log(SpawnManager.GetLocalPlayerObject().transform);
        // userList.Value.Add(clientId, GameManager.Instance.MyPlayerInfo);
        PlayerInputController.Instance.MyPlayerTransform = SpawnManager.GetLocalPlayerObject().transform;
        OnClientConnectedCallback -= OnClientJoined;
    }

    public void SetPort(ushort port){
        GetComponent<UnityTransport>().ConnectionData.Port = port;
    }

    public void JoinHost()
    {
        Debug.Log($"Welcome {GameManager.Instance.MyPlayerInfo.username}");
        StartHost();
    }

    public void JoinClient()
    {
        Debug.Log($"Welcome {GameManager.Instance.MyPlayerInfo.username}");
        StartClient();
    }


}

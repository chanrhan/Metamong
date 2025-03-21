using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 클라이언트 정보를 관리하는 관리자
/// </summary>
public class ClientManager : MonobehaviourSingleton<ClientManager>
{

    // [SerializeField]
    public ClientInfo ClientInfo;

    public string JoinCode;


    private GameObject myPlayerObject;
    private NetworkObject playerNetworkObject;
    private PlayerController playerController;

    public GameObject MyPlayerObject{
        get{
            return myPlayerObject;
        }
        set{
            myPlayerObject = value;
            playerNetworkObject = value.GetComponent<NetworkObject>();
            playerController = value.GetComponent<PlayerController>();

            // Join 3D Channel
            VivoxManager.Instance.Join3DChannel(value);

            // Join Voice Channel
            // VivoxManager.Instance.JoinVoiceChannel();
        }
    }

    public NetworkObject PlayerNetworkObject{
        get => playerNetworkObject;
    }

    public PlayerController PlayerController{
        get => playerController;
    }
    
}

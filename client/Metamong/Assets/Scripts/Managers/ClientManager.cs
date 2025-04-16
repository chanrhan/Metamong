using LLMUnity;
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
    public ClientInfo ClientInfo = new ClientInfo();

    public string JoinCode = "";


    private GameObject myPlayerObject = null;
    private NetworkObject playerNetworkObject = null;
    private PlayerController playerController = null;

    public GameObject MyPlayerObject{
        get{
            return myPlayerObject;
        }
         set{
            myPlayerObject = value;
            playerNetworkObject = value.GetComponent<NetworkObject>();
            playerController = value.GetComponent<PlayerController>();

            // Join 3D Channel
            // VivoxManager.Instance.Join3DChannel(value);

            // Join Voice Channel
            JoinVoiceChannel();
        }
    }

    public NetworkObject PlayerNetworkObject{
        get => playerNetworkObject;
    }

    public PlayerController PlayerController{
        get => playerController;
    }
    

    void Start()
    {
        ClientInfo = new ClientInfo();
        JoinCode = "";
        myPlayerObject = null;
        playerNetworkObject = null;
        playerController = null;
    }

    private async void JoinVoiceChannel(){
        await VivoxManager.Instance.JoinVoiceChannel(JoinCode);
    }

    
}

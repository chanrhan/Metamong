using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 클라이언트 정보를 관리하는 관리자
/// </summary>
public class ClientManager : MonoBehaviour
{

    [SerializeField]
    public ClientInfo ClientInfo;

    public static ClientManager Instance { get; private set; }

    private GameObject myPlayerObject;
    private NetworkObject playerNetworkObject;

    public GameObject MyPlayerObject{
        get{
            return myPlayerObject;
        }
        set{
            myPlayerObject = value;
            playerNetworkObject = value.GetComponent<NetworkObject>();
        }
    }

    public NetworkObject PlayerNetworkObject{
        get{
            return playerNetworkObject;
        }
        private set{
            playerNetworkObject = value;
        }
    }

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }   
    }

    private void Start() {
        SceneManager.sceneLoaded += OnLoadInGameScene;
    }

    /// <summary>
    /// InGame 씬으로 접속 시, 아래 함수 실행
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnLoadInGameScene(Scene scene, LoadSceneMode mode){
        if(scene.name.Equals("InGame")){
            if(ClientInfo.isHost){
                CustomNetworkManager.Instance.JoinHost();
            }else{
                CustomNetworkManager.Instance.JoinClient();
            }
            SceneManager.sceneLoaded -= OnLoadInGameScene;
        }
    }
}

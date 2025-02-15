using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance { get; private set; }

    [SerializeField]
    private ClientInfo clientInfo = new ClientInfo();

    public ClientInfo ClientInfo{get{ return clientInfo;} set{clientInfo = value;}}

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

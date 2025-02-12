using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private MyPlayerInfo myPlayerInfo;

    public MyPlayerInfo MyPlayerInfo{get{ return myPlayerInfo;} set{myPlayerInfo = value;}}

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MyPlayerInfo = new MyPlayerInfo();
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
            if(MyPlayerInfo.isHost){
                CustomNetworkManager.Instance.JoinHost();
            }else{
                CustomNetworkManager.Instance.JoinClient();
            }
            SceneManager.sceneLoaded -= OnLoadInGameScene;
        }
    }
}

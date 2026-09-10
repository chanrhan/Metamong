using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonobehaviourSingleton<GameSceneManager>
{
    [SerializeField]
    private string lobbySceneName = "Lobby"; // 로비 씬 이름

    [SerializeField]
    private string inGameSceneName = "InGame"; // 인게임 씬 이름 

    public string LobbySceneName{
        get=>lobbySceneName;
    }

    public string InGameSceneName{
        get=>inGameSceneName;
    }


    public Action OnLoginEnd;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnLoadInGameScene;       
        SceneManager.sceneLoaded += OnLoadLobbyScene;       
    }

    public void LoadInGameScene(){
        SceneManager.LoadScene(inGameSceneName);
    }

    public void LoadLobbyScene(){
        SceneManager.LoadScene(lobbySceneName);
    }

    /// <summary>
    /// InGame 씬으로 접속 시, 아래 함수 실행
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnLoadInGameScene(Scene scene, LoadSceneMode mode){
        if(scene.name.Equals(inGameSceneName)){
            CustomNetworkManager.Instance.Join();

            // SceneManager.sceneLoaded -= OnLoadInGameScene;
        }
    }

    private void OnLoadLobbyScene(Scene scene, LoadSceneMode mode){
        if(scene.name.Equals(lobbySceneName)){
            // do something
        }
    }
}

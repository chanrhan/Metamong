using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonobehaviourSingleton<GameSceneManager>
{
    [SerializeField]
    private string inGameSceneName = "InGame"; // 인게임 씬 이름 

    public Action OnLoginEnd;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnLoadInGameScene;       
    }

    public void LoadInGameScene(){
        SceneManager.LoadScene(inGameSceneName);
    }

    /// <summary>
    /// InGame 씬으로 접속 시, 아래 함수 실행
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnLoadInGameScene(Scene scene, LoadSceneMode mode){
        if(scene.name.Equals(inGameSceneName)){
            CustomNetworkManager.Instance.Join();

            SceneManager.sceneLoaded -= OnLoadInGameScene;
        }
    }
}

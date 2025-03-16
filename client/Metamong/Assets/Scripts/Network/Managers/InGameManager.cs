using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 인게임 매니저
/// 인게임 관련해서 클라이언트 개별로 독자적으로 관리할 것들을 모아놓는 관리자이다.
/// </summary>
public class InGameManager : MonobehaviourSingleton<InGameManager>
{
    public void Disconnect(){
        CustomNetworkManager.Instance.Disconnect();
        SceneManager.LoadScene("Login");
    }
}

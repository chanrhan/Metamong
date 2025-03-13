using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginHandler : MonoBehaviour
{
    [SerializeField]
    private string LoadedSceneName = "InGame"; // 인게임 씬 이름 

    private TMP_InputField usernameInput;
    private TMP_InputField portInput;
    private Button btnClient;
    private Button btnHost;

    private void Awake() {
        TMP_InputField[] inputs = GetComponentsInChildren<TMP_InputField>();
        portInput = inputs[0];
        usernameInput = inputs[1];

        Button[] buttons = GetComponentsInChildren<Button>();
        btnClient = buttons[0];
        btnHost = buttons[1];

        btnClient.onClick.AddListener(()=>{
            Login(false);
        });

        btnHost.onClick.AddListener(()=>{
            Login(true);
        });
    }

    /// <summary>
    /// 로그인하는 함수
    /// 클라이언트 정보에 username과 호스트 여부를 등록 
    /// </summary>
    /// <param name="isHost">호스트 여부</param>
    private void Login(bool isHost){
        ClientManager.Instance.ClientInfo.username = usernameInput.text;
        ClientManager.Instance.ClientInfo.isHost = isHost;

        // 포트 설정 
        ushort port;
        if(ushort.TryParse(portInput.text, out port)){
            CustomNetworkManager.Instance.SetPort(port);
        }

        // 인게임 씬으로 로드 
        SceneManager.LoadScene(LoadedSceneName);
    }
}

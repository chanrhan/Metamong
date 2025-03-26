using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
// 로그인 핸들러
// 로그인 관련 정보들을 저장한다.
/// </summary>
public class LoginHandler : MonoBehaviour
{
    private TMP_InputField usernameInput;
    private TMP_InputField joinCodeInput;
    private Button loginClientButton;
    private Button loginHostButton;
    
    private void Awake() {
        TMP_InputField[] inputs = GetComponentsInChildren<TMP_InputField>();
        joinCodeInput = inputs[0];
        usernameInput = inputs[1];

        Button[] buttons = GetComponentsInChildren<Button>();
        loginClientButton = buttons[0];
        loginHostButton = buttons[1];

        loginClientButton.onClick.AddListener(()=>{
            Login(false);
        });

        loginHostButton.onClick.AddListener(()=>{
            Login(true);
        });
    }

    void Start()
    {
        int randomInt = UnityEngine.Random.Range(0, 30);
        char r = (char)(65 + randomInt);
        usernameInput.text = $"{r}{r}{r}";
    }

    /// <summary>
    /// 자신의 컴퓨터가 현재 연결되어 있는 로컬 IP 주소를 반환 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">IP 주소를 못 찾으면 예외 처리</exception>
    public string GetLocalIPAddress()
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up &&
                (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || 
                 ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
            }
        }
        throw new Exception("No Network Available");
    }

    

    /// <summary>
    /// 로그인하는 함수
    /// 클라이언트 정보에 username과 호스트 여부를 등록 
    /// </summary>
    /// <param name="isHost">호스트 여부</param>
    private void Login(bool isHost){
        ClientManager.Instance.ClientInfo.username = usernameInput.text;
        ClientManager.Instance.ClientInfo.isHost = isHost;
        ClientManager.Instance.JoinCode = joinCodeInput.text;

        AuthenticationManager.Instance.Login();
    }

    
}

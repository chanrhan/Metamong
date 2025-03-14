using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginHandler : MonoBehaviour
{
    [SerializeField]
    private string LoadedSceneName = "InGame"; // 인게임 씬 이름 

    private TMP_InputField ipInput;
    private TMP_InputField portInput;
    private TMP_InputField usernameInput;
    private Button btnClient;
    private Button btnHost;

    private List<string> randomUsernameList = new List<string>{
        "Koryong",
        "EEEE",
        "GOGO",
        "HeLlOwOrLd",
        "HotGay",
        "gayRoll",
        "ILOVEJANG",
        "WHIP",
        "OPENAI",
        "FuxkingApple",
        "GoodGood",
        "RunningAppeach",
        "DuksuPasta",
        "Suppman",
        "WELCOME",
        "IDIOT",
        "SLAVES",
    };

    private void Awake() {
        TMP_InputField[] inputs = GetComponentsInChildren<TMP_InputField>();
        ipInput = inputs[0];
        portInput = inputs[1];
        usernameInput = inputs[2];

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

    void Start()
    {
        int randomInt = UnityEngine.Random.Range(0, randomUsernameList.Count);
        usernameInput.text = randomUsernameList[randomInt];
    }

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

        // 포트 설정 
        ushort port;
        string ipAddress = "";
        if(isHost){
            ipAddress = GetLocalIPAddress();
        }else{
            ipAddress = ipInput.text;
        }
        if(ushort.TryParse(portInput.text, out port)){
            CustomNetworkManager.Instance.SetUnityTransport(ipAddress, port);
        }

        // 인게임 씬으로 로드 
        SceneManager.LoadScene(LoadedSceneName);
    }
}

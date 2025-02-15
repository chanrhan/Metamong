using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginHandler : MonoBehaviour
{
    [SerializeField]
    private string LoadedSceneName = "InGame";

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

    private void Login(bool isHost){
        ClientManager.Instance.ClientInfo.username = usernameInput.text;
        ClientManager.Instance.ClientInfo.isHost = isHost;

        ushort port;
        if(ushort.TryParse(portInput.text, out port)){
            CustomNetworkManager.Instance.SetPort(port);
        }

        SceneManager.LoadScene(LoadedSceneName);
    }
}

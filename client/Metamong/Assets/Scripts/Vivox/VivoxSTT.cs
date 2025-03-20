using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class VivoxSTT : MonobehaviourSingleton<VivoxSTT>
{

    [Header("STT Test")]
    [SerializeField]
    private Button testLoginButton;
    [SerializeField]
    private Button doSomethingButton;
    [SerializeField]
    private TextMeshProUGUI chatLog;
    [SerializeField]
    private TextMeshProUGUI users;

    [SerializeField]
    private GameObject spinningPrefab;

    private List<string> userList = new List<string>();

    void Start()
    {
        if(testLoginButton){
            testLoginButton.onClick.AddListener(VivoxManager.Instance.LoginVivox);
        }
        if(doSomethingButton){
            doSomethingButton.onClick.AddListener(Do);
        }
        VivoxManager.Instance.OnLoginEndEvent += Init;
    }

    private async void Init()
    {
        VivoxService.Instance.ParticipantAddedToChannel += (participant)=>{
            userList.Add(participant.DisplayName);
            UpdateUserList();
        };

        VivoxService.Instance.ParticipantRemovedFromChannel += (participant)=>{
            userList.Remove(participant.DisplayName);
            UpdateUserList();
        };

        VivoxService.Instance.SpeechToTextMessageReceived += OnSpeechTotextMessageReceived;
        await VivoxManager.Instance.JoinVoiceChannel();

        GameObject runObject = Instantiate(spinningPrefab);
    
    }


    private void Do(){
        Debug.Log(VivoxService.Instance.IsSpeechToTextEnabled(VivoxManager.Instance.channelName));
    }

    private void UpdateUserList(){
        StringBuilder sb = new StringBuilder();
        foreach(string name in userList){
            sb.Append(name).Append("\n");
        }
        users.text = sb.ToString();
    }


    private void OnSpeechTotextMessageReceived(VivoxMessage message){
        Debug.Log("STT Received: " + message.MessageText);
        chatLog.text += message.MessageText;
    }

}

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Vivox;
using Unity.Services.Vivox.AudioTaps;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Services.Vivox.AudioTaps
{
    public class VivoxTest : MonobehaviourSingleton<VivoxTest>
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



        private VivoxChannelAudioTap channelAudioTap;

        private List<string> userList = new List<string>();

        protected override void Awake()
        {
            base.Awake();
            channelAudioTap = GetComponent<VivoxChannelAudioTap>();
        }

        void Start()
        {
            if(testLoginButton){
                testLoginButton.onClick.AddListener(Login);
            
            }
            if(doSomethingButton){
                doSomethingButton.onClick.AddListener(Do);
            }
            VivoxManager.Instance.OnLoginEndEvent += Init;

        
        }

        private void Login(){
            LoginAsync();
        }

        private async void LoginAsync(){
            await AuthenticationUtils.Authenticate();
            await VivoxManager.Instance.InitializeVivox();
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

            SetSpeechToText();
            await VivoxManager.Instance.JoinVoiceChannel("test");

            GameObject runObject = Instantiate(spinningPrefab);
        }

        private void SetSpeechToText(){
            VivoxService.Instance.SpeechToTextMessageReceived += OnSpeechTotextMessageReceived;
        }

        private void Do(){
            // Debug.Log(VivoxService.Instance.IsSpeechToTextEnabled(VivoxManager.Instance.channelName));
            
        }

        private void UpdateUserList(){
            StringBuilder sb = new StringBuilder();
            foreach(string name in userList){
                sb.Append($"<{name}>").Append("\n");
            }
            users.text = sb.ToString();
        }


        private void OnSpeechTotextMessageReceived(VivoxMessage message){
            Debug.Log("STT Received: " + message.MessageText);
            chatLog.text += message.MessageText;
        }

    }
}


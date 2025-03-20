using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine.UI;

public class VivoxManager : MonobehaviourSingleton<VivoxManager>
{
    public event Action OnLoginEndEvent;

    [SerializeField]
    public string channelName;
    [SerializeField]
    private Channel3DSetting channel3DSetting;
    [SerializeField]
    private float positionUpdateRate = 0.5f;
    [SerializeField]
    private Image progressBar;

    private float progressGage = 0f;

    private HashSet<VivoxParticipant> joinedParticipants = new HashSet<VivoxParticipant>();
    private HashSet<VivoxParticipant> speakingParticipants = new HashSet<VivoxParticipant>();
    

    public HashSet<VivoxParticipant> JoinedParticipants{
        get => joinedParticipants;
    }

    public VivoxParticipant[] SpeakingParticipants{
        get => joinedParticipants.Where(participant=>participant.AudioEnergy > 0).ToArray();
        
    }

    private IEnumerator StartProgressCoroutine(){
        progressBar.fillAmount = 0f;
        while(progressBar.fillAmount < 1f){
            if(progressBar.fillAmount > progressGage){
                yield return new WaitForSeconds(0.5f);
            }
            progressBar.fillAmount += 0.02f;
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void SetLoginProgress(int amount){
        if(progressGage == 0){
            StartCoroutine(StartProgressCoroutine());
        }
        progressGage = (float) amount / 100;
    }

    public async void LoginVivox(){
        SetLoginProgress(10);
        await UnityServices.InitializeAsync();
        SetLoginProgress(30);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        SetLoginProgress(50);
        await VivoxService.Instance.InitializeAsync();
        SetLoginProgress(70);

        Debug.Log("초기화 완료");

        BindSessionEvents();

        await LoginAsync();
        SetLoginProgress(98);

        // Vivox 음향 에코 제거 
        VivoxService.Instance.EnableAcousticEchoCancellation();

        Debug.Log("로그인 완료");

        OnLoginEndEvent?.Invoke();
        progressBar.gameObject.SetActive(false);
    }

    private async Task LoginAsync(){
        LoginOptions options = new LoginOptions();
        // options.DisplayName = Guid.NewGuid().ToString();
        options.DisplayName = ClientManager.Instance?.ClientInfo.username ?? Guid.NewGuid().ToString();
        options.SpeechToTextLanguages = new List<string>{
            "ko","kr"
        };

        await VivoxService.Instance.LoginAsync(options);
    }

    

    private void BindSessionEvents(){
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
    }

    private void OnChannelMessageReceived(VivoxMessage vivoxMessage){
        ChatManager.Instance.InputChat(vivoxMessage.SenderDisplayName, vivoxMessage.MessageText);
    }

    

    public async Task JoinVoiceChannel(){
        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);

        // STT 
        await VivoxService.Instance.SpeechToTextEnableTranscription(channelName);
    }

    public async void Join3DChannel(GameObject speakObj){
        await VivoxService.Instance.JoinPositionalChannelAsync(channelName, ChatCapability.AudioOnly, channel3DSetting.GetChannel3DSetting());

        StartCoroutine(Update3DPositionCoroutine(speakObj));
    }

    private IEnumerator Update3DPositionCoroutine(GameObject speakObj){
        while(true){
            VivoxService.Instance.Set3DPosition(speakObj, channelName);
            yield return new WaitForSeconds(positionUpdateRate);
        }
    }

    private void OnParticipantAdded(VivoxParticipant participant){
        Debug.Log("Vivox Participant Added : " + participant.DisplayName);
        participant.SetLocalVolume(100);
        joinedParticipants.Add(participant);
    }

    private void OnParticipantRemoved(VivoxParticipant participant){
        Debug.Log("Vivox Participant Removed : " + participant.DisplayName);
        joinedParticipants.Remove(participant);
    }

}

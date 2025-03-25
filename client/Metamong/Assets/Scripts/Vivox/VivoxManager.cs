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
using Unity.Services.Vivox.AudioTaps;

public class VivoxManager : MonobehaviourSingleton<VivoxManager>
{
    public event Action OnLoginEndEvent;

    [SerializeField]
    public string channelName;
    [SerializeField]
    private Channel3DSetting channel3DSetting;
    [SerializeField]
    private float positionUpdateRate = 0.5f;

    [Header("Vivox Configuration Options")]
    [SerializeField]
    private bool enableAdvancedAutoLevels = true;
    [SerializeField]
    private bool enableDtx = true;
    [SerializeField]
    private VivoxLogLevel logLevel = VivoxLogLevel.Debug;

    private HashSet<VivoxParticipant> joinedParticipants = new HashSet<VivoxParticipant>();
    private HashSet<VivoxParticipant> speakingParticipants = new HashSet<VivoxParticipant>();
    

    public HashSet<VivoxParticipant> JoinedParticipants{
        get => joinedParticipants;
    }

    public VivoxParticipant[] SpeakingParticipants{
        get => joinedParticipants.Where(participant=>participant.AudioEnergy > 0).ToArray();
    }

    public async Task InitializeVivox(){
        await VivoxService.Instance.InitializeAsync(new VivoxConfigurationOptions{
            EnableAdvancedAutoLevels = enableAdvancedAutoLevels,
            LogLevel = logLevel,
            EnableDtx = enableDtx,
            // UpstreamJitterFrameCount = upstreamJitterFrameCount
        });
        AuthenticationManager.Instance.SetLoginProgress(50);

        Debug.Log("초기화 완료");

        BindSessionEvents();

        await LoginAsync();
        AuthenticationManager.Instance.SetLoginProgress(70);

        
        SetVoiceProperties();
        

        Debug.Log("로그인 완료");

        OnLoginEndEvent?.Invoke();
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

    private async void SetVoiceProperties(){
        // Vivox 음향 에코 제거 
        VivoxService.Instance.EnableAcousticEchoCancellation();

        await VivoxService.Instance.EnableAutoVoiceActivityDetectionAsync();
    }

    private void SetAutoVad(){
        
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
        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.TextAndAudio);

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

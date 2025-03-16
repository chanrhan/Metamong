using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System;
using System.Linq;

public class VivoxManager : MonobehaviourSingleton<VivoxManager>
{
    public event Action OnLoginEndEvent;

    [SerializeField]
    private string channelName;
    [SerializeField]
    private Channel3DSetting channel3DSetting;
    [SerializeField]
    private float positionUpdateRate = 0.5f;

    private HashSet<VivoxParticipant> joinedParticipants = new HashSet<VivoxParticipant>();

    public HashSet<VivoxParticipant> JoinedParticipants{
        get => joinedParticipants;
    }

    public VivoxParticipant[] ActiveParticipants{
        get => joinedParticipants.Where(participant=>participant.SpeechDetected).ToArray();
    }

    void Update()
    {
        
    }

    public async void LoginVivox(){
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await VivoxService.Instance.InitializeAsync();

        Debug.Log("초기화 완료");

        BindSessionEvents();

        await LoginAsync();

        Debug.Log("로그인 완료");

        OnLoginEndEvent?.Invoke();
    }

    private async Task LoginAsync(){
        LoginOptions options = new LoginOptions();
        options.DisplayName = Guid.NewGuid().ToString();

        await VivoxService.Instance.LoginAsync(options);
    }

    

    private void BindSessionEvents(){
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantRemoved;

        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
    }

    private void OnChannelMessageReceived(VivoxMessage vivoxMessage){
        ChatManager.Instance.InputChat(vivoxMessage.SenderDisplayName, vivoxMessage.MessageText);
    }

    public async void JoinVoiceChannel(){
        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);
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
        joinedParticipants.Add(participant);
    }

    private void OnParticipantRemoved(VivoxParticipant participant){
        Debug.Log("Vivox Participant Removed : " + participant.DisplayName);
        joinedParticipants.Remove(participant);
    }

}

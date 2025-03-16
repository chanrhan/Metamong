using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Vivox;
using UnityEngine;

public class Voice3DChannelUI : MonoBehaviour
{
    private VivoxSpeakerUI[] vivoxSpeakers;
    
    void Awake()
    {
        vivoxSpeakers = GetComponentsInChildren<VivoxSpeakerUI>();           
    }

    public void UpdateParticipantsVoiceUI(){
        VivoxParticipant[] activeSpeakers = VivoxManager.Instance.ActiveParticipants;

        for(int i=0;i<vivoxSpeakers.Count();++i){
            VivoxParticipant participant = activeSpeakers?[i];
            Debug.Log("Speaker: " + participant.DisplayName);
            if(participant != null){
                vivoxSpeakers[i].DisplayOn();
                vivoxSpeakers[i].SpeakerName = participant.DisplayName;
                vivoxSpeakers[i].SpeakerVolume = participant.LocalVolume;
            }else{
                vivoxSpeakers[i].DisplayOff();
            }
        }
    }
}

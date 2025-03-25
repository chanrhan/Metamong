using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Vivox;
using UnityEngine;

public class Voice3DChannelUI : MonoBehaviour
{
    [SerializeField]
    private VivoxSpeakerUI[] vivoxSpeakerUIs;
    
    void Awake()
    {
        vivoxSpeakerUIs = GetComponentsInChildren<VivoxSpeakerUI>();           
        foreach(VivoxSpeakerUI vivoxSpeakerUI in vivoxSpeakerUIs){
            vivoxSpeakerUI.DisplayOff();
        }
    }

    public void UpdateUI(){
        int i=0;
        foreach(VivoxParticipant participant in VivoxManager.Instance.JoinedParticipants){
            // Debug.Log("AudioEnergy: "+participant.AudioEnergy);
            vivoxSpeakerUIs[i].DisplayOn();
            vivoxSpeakerUIs[i].SpeakerName = participant.DisplayName;
            vivoxSpeakerUIs[i].SetVolume(participant.AudioEnergy);
            // else{
            //     vivoxSpeakerUIs[i].DisplayOff();
            // }
            ++i;
        }

        for(;i<vivoxSpeakerUIs.Count();++i){
            vivoxSpeakerUIs[i].DisplayOff();
        }
    }
}

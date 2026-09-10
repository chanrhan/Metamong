using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;

public class VoiceChannelUI : MonoBehaviour
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
            if (CustomNetworkManager.Instance.TryGetNetworkObjectByClientId((ulong)i, out NetworkObject no))
            {
                if (no.TryGetComponent(out PlayerController pc))
                {
                    no.gameObject.GetComponent<PlayerController>().AddNameTag(participant.DisplayName);
                }
                else
                {
                    Debug.Log($"User Not Found : {i}");
                }
               
            }
            ++i;
        }

        for(;i<vivoxSpeakerUIs.Count();++i){
            vivoxSpeakerUIs[i].DisplayOff();
        }
    }
}

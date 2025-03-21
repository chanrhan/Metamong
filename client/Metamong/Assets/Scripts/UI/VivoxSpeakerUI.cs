using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class VivoxSpeakerUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI speakerNameTMP;
    [SerializeField]
    private Image onMicImg;

    public string SpeakerName{
        get => speakerNameTMP.text;
        set => speakerNameTMP.text = value;
    }

    public void SetVolume(double audioEnergy){
        if(audioEnergy > 0){
            onMicImg.gameObject.SetActive(true);
        }else{
            onMicImg.gameObject.SetActive(false);
        }
    }


    public void DisplayOn(){
        gameObject.SetActive(true);
    }

    public void DisplayOff(){
        gameObject.SetActive(false);
    }
}

using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class VivoxSpeakerUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI speakerNameTMP;
    [SerializeField]
    private TextMeshProUGUI volumeTMP;

    public string SpeakerName{
        get => speakerNameTMP.text;
        set => speakerNameTMP.text = value;
    }

    public double SpeakerVolume{
        get => double.Parse(volumeTMP.text);
        set => volumeTMP.text = (value * 100).ToString("F1");
    }

    public void DisplayOn(){
        gameObject.SetActive(true);
    }

    public void DisplayOff(){
        gameObject.SetActive(false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VivoxSpeakerUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI speakerNameTMP;
    [SerializeField]
    private TextMeshProUGUI volumeTMP;

    private MeshRenderer meshRenderer;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public string SpeakerName{
        get => speakerNameTMP.text;
        set => speakerNameTMP.text = value;
    }

    public int SpeakerVolume{
        get => int.Parse(volumeTMP.text);
        set => volumeTMP.text = value.ToString();
    }

    public void DisplayOn(){
        meshRenderer.enabled = true;
    }

    public void DisplayOff(){
        meshRenderer.enabled = false;
    }
}

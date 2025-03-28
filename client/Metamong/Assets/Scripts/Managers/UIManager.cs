using System.Collections;
using System.Drawing;
using TMPro;
using UnityEngine;

public class UIManager : MonobehaviourSingleton<UIManager>
{
    [Header("Channel Info")]
    [SerializeField]
    private ChannelInfoPanel channelInfoPanel;

    [Header("Whisper")]
    [SerializeField]
    private TextMeshProUGUI sttResponse;
    [SerializeField]
    private Image vadIndicatorImage;

    [Header("Vivox Voice Channel")]
    [SerializeField]
    private VoiceChannelUI voiceChannelUI;
    private float voiceChannelUIUpdateInterval = 0.1f;

    void Start()
    {
        ClearSttResponseText();
    }

    public void SetSttResponseText(string text){
        sttResponse.text = text;
    }

    public void AddSttResponseText(string text){
        sttResponse.text += text;
    }

    public void ClearSttResponseText(){
        sttResponse.text = "";
    }

    public void SetChannelCode(string channelCode){
        channelInfoPanel.ChannelCode = channelCode;
    }

    public void StartUpdatingVoiceChannelUI(){
        StartCoroutine(UpdateVoiceChannelUICoroutine());  
    }

    private IEnumerator UpdateVoiceChannelUICoroutine(){
        while(true){
            voiceChannelUI.UpdateUI();
            yield return new WaitForSeconds(voiceChannelUIUpdateInterval);
        }
    }
    
}

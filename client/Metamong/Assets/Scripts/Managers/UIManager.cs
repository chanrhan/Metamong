using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Header UI")]

    [SerializeField]
    private Button btnQuit;
    [SerializeField]
    private GameObject loadingPanel;

    protected override void Awake()
    {
        base.Awake();
        loadingPanel.SetActive(true);
        if(btnQuit){
            btnQuit.onClick.AddListener(Quit);
        }
    }

    void Start()
    {
        ClearSttResponseText();
    }

    public void HideLoadingPanel(){
        loadingPanel.SetActive(false);
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

    private void Quit(){
        CustomNetworkManager.Instance.Disconnect();
        GameSceneManager.Instance.LoadLobbyScene();
    }
}

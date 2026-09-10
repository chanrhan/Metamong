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
    private Image vadImg;
    [SerializeField]
    private Image onRecordPanel;
    [SerializeField]
    private Color orgColor;
    [SerializeField]
    private Color onRecordColor;
    

    [Header("Vivox Voice Channel")]
    [SerializeField]
    private VoiceChannelUI voiceChannelUI;
    private float voiceChannelUIUpdateInterval = 0.1f;

    [Header("Header UI")]

    [SerializeField]
    private Button btnQuit;
    [SerializeField]
    private GameObject loadingPanel;

    public TMP_Text sttOrg;

    protected override void Awake()
    {
        base.Awake();
        loadingPanel.SetActive(true);
        if (btnQuit)
        {
            btnQuit.onClick.AddListener(Quit);
        }
        STTManager.Instance.OnVadChanged = OnVoiceDetected;
        STTManager.Instance.OnRecord = OnRecord;
        // STTManager.Instance.OnStopRecord = OnStopRecord;
        
    }

    void Start()
    {
        ClearSttResponseText();
    }

    public void OnRecord(bool isRecord)
    {
        Color alphaControl = onRecordPanel.color;
        Color textAlpha = sttOrg.color;
        //onRecordPanel.CrossFadeAlpha(1f, 1f, ignoreTimeScale: false);
        alphaControl = isRecord ? onRecordColor : orgColor;
        alphaControl.a = isRecord ? 1f : 0.2f;
        textAlpha.a = isRecord ? 1f : 0.2f;

        onRecordPanel.color = alphaControl;
        sttOrg.color = textAlpha;
    }

    public void OnVoiceDetected(bool isDeteched)
    {
        vadImg.gameObject.SetActive(isDeteched);
    }

    // public void OnStopRecord(bool isStopped)
    // {
    //     onRecordPanel.CrossFadeAlpha(0.2f, 1f, ignoreTimeScale: false);
    // }

    public void HideLoadingPanel()
    {
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
        VivoxManager.Instance.QuitVoiceChannel();
        CustomNetworkManager.Instance.Disconnect();
        GameSceneManager.Instance.LoadLobbyScene();
    }
}

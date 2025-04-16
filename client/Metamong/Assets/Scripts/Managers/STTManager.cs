using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using System.Threading.Tasks;

public class SegmentMotionSet{
    public string segment;
    public string actionClipName;
    public string faceClipName;

    public SegmentMotionSet(string segment){
        this.segment = segment;
    }
    public SegmentMotionSet(string segment, string actionClipName, string faceClipName){
        this.segment = segment;
        this.actionClipName = actionClipName;
        this.faceClipName = faceClipName;
    }

    public override String ToString(){
        return "seg: "+segment + ", action: " + actionClipName + ", face: " + faceClipName;
    }
}

public class STTManager : MonobehaviourSingleton<STTManager>
{
    // [SerializeField]
    private WhisperManager whisper;
    // [SerializeField]
    private MicrophoneRecord microphoneRecord;
    private WhisperStream _stream;

    private PlayerController playerController;
    public PlayerController PlayerController{
        set => playerController = value;
    }
    

    [Header("Mac Os")]
    [SerializeField]
    private bool AllowMacOs = false;
    private bool IsPlatformMacOs = false;

    public static int segmentId = 0;

    private List<SegmentMotionSet> segmentMotionSets = new List<SegmentMotionSet>();
    public List<SegmentMotionSet> SegmentMotionSets{
        get=>segmentMotionSets;
    }

    public List<double> SegmentFinshiedTimes{
        get=>_stream.finishSegmentTime;
    }

    private int lastSegmentId = 0;

    protected async override void Awake()
    {
        base.Awake();

        IsPlatformMacOs = !AllowMacOs && 
        (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor);

        // Debug.Log("OS : " + Application.platform);
        
        if(IsPlatformMacOs){
            Debug.LogWarning("MacOS should not use Whisper!");
           return; 
        }
        
        whisper = GetComponent<WhisperManager>();
        microphoneRecord = GetComponent<MicrophoneRecord>();
        

        if(whisper == null){
            throw new Exception("WhisperManager is not found!");
        }
        if(microphoneRecord == null){
            throw new Exception("MicrophoneRecord is not found");
        }

        _stream = await whisper.CreateStream(microphoneRecord);

        if(_stream == null){
            throw new Exception("CreateStream returned Invalid Value: " + _stream);
        }
        _stream.OnResultUpdated += OnResult;
        _stream.OnSegmentUpdated += OnSegmentUpdated;
        _stream.OnSegmentFinished += OnSegmentFinished;
        // _stream.OnStreamFinished += OnFinished;

        // myCharic = ClientManager.Instance.PlayerController;
    }

    void Update()
    {
        if(IsPlatformMacOs){
            return;
        }


        if(Input.GetKeyDown(KeyCode.T)){
            Debug.Log("Start Record");
            StartRecord();
        }else if(Input.GetKeyUp(KeyCode.T)){
            Debug.Log("Stop Record");
            StopRecord();
        }
    }

    public void StartRecord(){
        _stream.StartStream();
        microphoneRecord.StartRecord();
    }

    public void StopRecord(){
        microphoneRecord.StopRecord();
    }

    private void OnResult(string result){
        UIManager.Instance.SetSttResponseText(result);
    }

    public void OnInputDeviceChanged(string deviceName){
        if(IsPlatformMacOs){
            return;
        }

        if(microphoneRecord != null){
            microphoneRecord.SelectedMicDevice = deviceName;
        }
    }

    private void OnSegmentUpdated(WhisperResult segment)
    {
        LogSegmentMotion(segment.Result);
    }

    private async void LogSegmentMotion(string result){
        SegmentMotionSet segmentMotionSet = new SegmentMotionSet(result);
        await playerController.SendResultToLlama(segmentMotionSet);
        print(segmentMotionSet.ToString());
        segmentMotionSets.Add(segmentMotionSet);
    }
    
    private void OnSegmentFinished(WhisperResult segment)
    {
        if (playerController != null)
        {
            //playerController.SendResultToLlama(segment.Result);
            Debug.Log($"Segment finished: {segment.Result}");
        }
        else
        {
            Debug.LogError("PlayerController 인스턴스가 할당되지 않았습니다.");
        }
    }

    
    private void OnFinished(string finalResult)
    {
        print("Stream finished!");
    }
    
}
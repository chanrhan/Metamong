using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Whisper;
using Whisper.Utils;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

public class SegmentMotionSet{
    public string segment;
    public string actionClipName;
    public string faceClipName;
    public bool timeout = false;
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

    public OnVadChangedDelegate OnVadChanged
    {
        set => microphoneRecord.OnVadChanged += value;
    }

    public Action<bool> onRecord;

    public Action<bool> OnRecord
    {
        set => onRecord += value;
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
    
    private bool isRecording = false;

    protected async override void Awake()
    {
        base.Awake();

        IsPlatformMacOs = !AllowMacOs &&
        (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor);

        // Debug.Log("OS : " + Application.platform);

        if (IsPlatformMacOs)
        {
            Debug.LogWarning("MacOS should not use Whisper!");
            return;
        }

        whisper = GetComponent<WhisperManager>();
        microphoneRecord = GetComponent<MicrophoneRecord>();


        if (whisper == null)
        {
            throw new Exception("WhisperManager is not found!");
        }
        if (microphoneRecord == null)
        {
            throw new Exception("MicrophoneRecord is not found");
        }

        _stream = await whisper.CreateStream(microphoneRecord);

        if (_stream == null)
        {
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
        if (SceneManager.GetActiveScene().name != GameSceneManager.Instance.InGameSceneName)
        {
            return;
        }

        if (IsPlatformMacOs)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            if (isRecording)
            {
                isRecording = false;
                Debug.Log("Stop Record");
                StopRecord();
            }
            else
            {
                isRecording = true;
                Debug.Log("Start Record");
                StartRecord();
            }
            onRecord?.Invoke(isRecording);
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
    }

    private void OnSegmentFinished(WhisperResult segment)
    {
        MotionGenerator.Instance?.Generate(segment.Result);
    }

    
    private void OnFinished(string finalResult)
    {
        print("Stream finished!");
    }
    
}
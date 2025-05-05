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
using System.Linq;
using Unity.VisualScripting;

public class STTManager : MonobehaviourSingleton<STTManager>
{

    private const string LOG_PATH = "whisper_log.json";
    // [SerializeField]
    private WhisperManager wm;
    // [SerializeField]
    private MicrophoneRecord microphoneRecord;
    private WhisperStream _stream;
    private WhisperWrapper whisperWrapper;

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

    private static List<string> ignoredSegements = new List<string>();

    private List<FileLogVO.LogContextItem> logContextItems = new List<FileLogVO.LogContextItem>();
    public List<FileLogVO.LogContextItem> LogContextItems{
        get=>logContextItems;
    }

    public List<double> SegmentFinshiedTimes{
        get=>_stream.finishSegmentTimes;
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

        wm = GetComponent<WhisperManager>();
        microphoneRecord = GetComponent<MicrophoneRecord>();


        if (wm == null)
        {
            throw new Exception("WhisperManager is not found!");
        }
        if (microphoneRecord == null)
        {
            throw new Exception("MicrophoneRecord is not found");
        }

        _stream = await wm.CreateStream(microphoneRecord);

        if (_stream == null)
        {
            throw new Exception("CreateStream returned Invalid Value: " + _stream);
        }
        _stream.OnResultUpdated += OnResult;
        _stream.OnSegmentUpdated += OnSegmentUpdated;
        _stream.OnSegmentFinished += OnSegmentFinished;
        // _stream.OnStreamFinished += OnFinished;

        // myCharic = ClientManager.Instance.PlayerController;

        whisperWrapper = wm.GetWhisperWrapper();
        if (whisperWrapper == null)
        {
            throw new Exception("whisperWrapper is not found!");
        }
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
                // Debug.Log("Stop Record");
                StopRecord();
            }
            else
            {
                isRecording = true;
                // Debug.Log("Start Record");
                StartRecord();
            }
            onRecord?.Invoke(isRecording);
        }

        if (microphoneRecord.IsVoiceDetected)
        {
            OnVoiceDeteched();
        }
    }

    public void StartRecord(){
        _stream.StartStream();
        microphoneRecord.StartRecord();
    }

    public void StopRecord()
    {
        microphoneRecord.StopRecord();
        LogCurrentItems();
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
        GenerateMotion(segment);
    }

    private void OnSegmentFinished(WhisperResult segment)
    {
        
    }

    private async void GenerateMotion(WhisperResult segment)
    {
        var result = await MotionGenerator.Instance?.Generate(segment.Result);
        switch (result.Item1)
        {
            case LogState.Ignored:
                ignoredSegements.Add(segment.Result);
                break;
            case LogState.Successed:
                FileLogVO.LogContextItem log = result.Item2;

                log.ignored = new List<string>(ignoredSegements);
                log.inferTime = segment.inferTime;
                log.totalTime = log.inferTime + log.llmTime + log.sbertTime;
                logContextItems.Add(log);
                ignoredSegements.Clear();
                break;
        }
        if (!isRecording)
        {
            LogCurrentItems();
        }
    }

    private void LogCurrentItems()
    {
        if (logContextItems.Count == 0)
        {
            return;
        }

        FileLogVO logVO = new FileLogVO
        {
            stepSec = wm.stepSec,
            keepSec = wm.keepSec,
            lengthSec = wm.lengthSec,
            logContextItems = logContextItems
        };

        FileLogUtils.Overwrite(logVO, LOG_PATH);
        logContextItems.Clear();
    }

    void OnVoiceDeteched()
    {
        MotionGenerator.Instance?.PlayTakingMotion();
    }

    void OnDestroy()
    {
        LogCurrentItems();
    }


    private void OnFinished(string finalResult)
    {
        print("Stream finished!");
    }
    
}
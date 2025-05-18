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
    [Header("Mac Os")]
    [SerializeField]
    private bool AllowMacOs = false;
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
    
    private bool IsPlatformMacOs = false;
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
        //_stream.OnSegmentUpdated += OnSegmentUpdated;
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

    public void StartRecord()
    {
        _stream.StartStream();
        microphoneRecord.StartRecord();

        MotionGenerator.Instance.StepSec = wm.stepSec;
        MotionGenerator.Instance.KeepSec = wm.keepSec;
        MotionGenerator.Instance.LengthSec = wm.lengthSec;
    }

    public void StopRecord()
    {
        microphoneRecord.StopRecord();
        MotionGenerator.Instance?.Log();
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
        MotionGenerator.Instance?.Generate(segment);
    }

    void OnVoiceDeteched()
    {
        MotionGenerator.Instance?.PlayTakingMotion();
    }

    void OnDestroy()
    {
        MotionGenerator.Instance?.Log();
    }


    private void OnFinished(string finalResult)
    {
        print("Stream finished!");
    }
    
}
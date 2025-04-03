using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whisper;
using Whisper.Utils;

public class STTManager : MonobehaviourSingleton<STTManager>
{
    // [SerializeField]
    private WhisperManager whisper;
    // [SerializeField]
    private MicrophoneRecord microphoneRecord;

    private WhisperStream _stream;

    private PlayerController myCharic;
    public PlayerController MyCharic
    {
        set {myCharic = value;}
    }

    [Header("Mac Os")]
    [SerializeField]
    private bool AllowMacOs = false;
    private bool IsPlatformMacOs = false;

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
        // _stream.OnSegmentUpdated += OnSegmentUpdated;
        _stream.OnSegmentFinished += OnSegmentFinished;
        // _stream.OnStreamFinished += OnFinished;
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
        print($"Segment updated: {segment.Result}");
    }
    
    private void OnSegmentFinished(WhisperResult segment)
    {
        myCharic.SendMessageToOthers(segment.Result);
        print($"Segment finished: {segment.Result}");
    }
    
    private void OnFinished(string finalResult)
    {
        print("Stream finished!");
    }
    
}
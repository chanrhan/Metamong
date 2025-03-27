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

    protected async override void Awake()
    {
        base.Awake();
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
        // _stream.OnSegmentFinished += OnSegmentFinished;
        // _stream.OnStreamFinished += OnFinished;
    }

    void Update()
    {
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
        print($"Segment finished: {segment.Result}");
    }
    
    private void OnFinished(string finalResult)
    {
        print("Stream finished!");
    }
    
}

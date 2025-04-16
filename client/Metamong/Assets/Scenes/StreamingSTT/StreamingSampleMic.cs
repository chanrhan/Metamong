using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using System.Collections.Generic;

namespace Whisper.Samples
{
    /// <summary>
    /// Stream transcription from microphone input.
    /// </summary>
    public class StreamingSampleMic : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
    
        [Header("UI")] 
        public Button button;
        public Text buttonText;
        public Text text;
        public ScrollRect scroll;
        private WhisperStream _stream;

        public List<string> segment_launch = new List<string>();
        
        public List<double> finSegTime = new List<double>()
;
        private async void Start()
        {
            _stream = await whisper.CreateStream(microphoneRecord);
            _stream.OnResultUpdated += OnResult;
            _stream.OnSegmentUpdated += OnSegmentUpdated;
            _stream.OnSegmentFinished += OnSegmentFinished;
            _stream.OnStreamFinished += OnFinished;

            microphoneRecord.OnRecordStop += OnRecordStop;
            button.onClick.AddListener(OnButtonPressed);
        }

        private void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                _stream.StartStream();
                microphoneRecord.StartRecord();
            }
            else
            {
                microphoneRecord.StopRecord();
            }
        
            buttonText.text = microphoneRecord.IsRecording ? "Stop" : "Record";
        }
    
        private void OnRecordStop(AudioChunk recordedAudio)
        {
            buttonText.text = "Record";
        }
    
        private void OnResult(string result)
        {
            text.text = result;
            UiUtils.ScrollDown(scroll);
        }
        
        private void OnSegmentUpdated(WhisperResult segment)
        {
            print($"Segment updated: {segment.Result}");
        }
        
        private void OnSegmentFinished(WhisperResult segment)
        {
            segment_launch.Add(segment.Result);
            finSegTime=_stream.finishSegmentTime;
            print($"Segment finished: {segment.Result}");
        }
        
        private void OnFinished(string finalResult)
        {
            print("Stream finished!");
            
            Debug.Log(string.Join(",", finSegTime));
            
        }
    }
}

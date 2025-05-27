using System;
using System.Collections.Generic;
using UnityEngine;
using Whisper;

public class LogTimelineItem{
    public int id;
    public float addToStreamTime;
    public bool useVad;
    public bool chunkVoiceDetected;
    public int step;
    public float slidingWindowTime;
    public float beforeInferTime;
    public float afterInferTime; // update time
    public float finishTime; // finish time
    public string segment;
}

public class LogWatchUtils : MonobehaviourSingleton<LogWatchUtils>
{
    [SerializeField]
    private string filename;
    private List<LogTimelineItem> timeline = new List<LogTimelineItem>();
    private WhisperStream whisperStream;

    [SerializeField]
    private int currId = 0;

    private LogTimelineItem timelineItem = new LogTimelineItem();

    private float _startTime = 0f;

    void Start()
    {
        STTManager.Instance.OnCreateWhisperStream += Init;
    }

    private void Init(WhisperStream whisperStream)
    {
        whisperStream = STTManager.Instance._stream;      
        if(whisperStream == null){
            UnityEngine.Debug.LogWarning("No whisper stream");
        }else{
            whisperStream.RecordAddToStream = (id)=>{
                if (id <= 1)
                {
                    _startTime = Time.realtimeSinceStartup;
                }
                currId = id;
                timeline.Add(new LogTimelineItem
                {
                    id = id,
                    addToStreamTime = Time.realtimeSinceStartup - _startTime 
                });
            };
            whisperStream.RecordUseVad = (id, useVad)=>
            {
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.useVad = useVad;
                }
            };
            whisperStream.RecordChunkVoiceDetected = (id, value)=>{
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.chunkVoiceDetected = value;
                }
            };
            whisperStream.RecordStep = (id, step)=>{
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.step = step;
                }
            };
            whisperStream.RecordSlidingWindow = (id)=>{
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.slidingWindowTime = Time.realtimeSinceStartup - _startTime;
                } 
            };
            whisperStream.RecordBeforeInfer = (id)=>{
                
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.beforeInferTime =  Time.realtimeSinceStartup - _startTime;
                } 
            };
            whisperStream.RecordAfterInfer = (id)=>{
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.afterInferTime =  Time.realtimeSinceStartup - _startTime;
                } 
            };
            whisperStream.RecordFinished = (id)=>{
               LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.finishTime =  Time.realtimeSinceStartup - _startTime;
                } 
            };
            whisperStream.RecordSegment = (id, seg)=>{
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.segment = seg;
                } 
                // AddTimeline();
            };
        }
    }
    
    // public void AddTimeline()
    // {
    //     timeline.Add(timelineItem);
    //     WriteFile();
    // }

    public void WriteFile()
    {
        FileLogUtils.OverwriteRange(timeline, "timeline/" + filename);
        timeline.Clear();
    }
}

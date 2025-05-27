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

    void Start()
    {
        whisperStream = STTManager.Instance._stream;      
        if(whisperStream == null){
            UnityEngine.Debug.LogWarning("No whisper stream");
        }else{
            whisperStream.RecordAddToStream = (id)=>{
                currId = id;
                timelineItem.addToStreamTime = Time.realtimeSinceStartup;
            };
            whisperStream.RecordUseVad = (id, useVad)=>{
                if(currId != id){
                    return;
                }
                timelineItem.useVad = useVad;
            };
            whisperStream.RecordChunkVoiceDetected = (id, value)=>{
                if(currId != id){
                    return;
                }
                timelineItem.chunkVoiceDetected = value;
            };
            whisperStream.RecordStep = (id, step)=>{
                if(currId != id){
                    return;
                }
                timelineItem.step = step;
            };
            whisperStream.RecordSlidingWindow = (id)=>{
                if(currId != id){
                    return;
                }
                timelineItem.slidingWindowTime = Time.realtimeSinceStartup;
            };
            whisperStream.RecordBeforeInfer = (id)=>{
                if(currId != id){
                    return;
                }
                timelineItem.beforeInferTime = Time.realtimeSinceStartup;
            };
            whisperStream.RecordAfterInfer = (id)=>{
                if(currId != id){
                    return;
                }
                timelineItem.afterInferTime = Time.realtimeSinceStartup;
            };
            whisperStream.RecordFinished = (id)=>{
                if(currId != id){
                    return;
                }
                timelineItem.finishTime = Time.realtimeSinceStartup;
            };
            whisperStream.RecordSegment = (id, seg)=>{
                if(currId != id){
                    return;
                }
                timelineItem.segment = seg;
                AddTimeline();
            };
        }
    }

    public void AddTimeline()
    {
        timeline.Add(timelineItem);
        WriteFile();
    }

    public void WriteFile(){
        FileLogUtils.Overwrite(timelineItem, "timeline/"+filename);
        timeline.Clear();
    }
}

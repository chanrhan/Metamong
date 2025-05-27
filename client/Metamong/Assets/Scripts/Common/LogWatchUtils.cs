using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Whisper;

public class LogTimelineItem
{
    public int id;
    public float addToStreamTime;
    public bool useVad;
    public bool chunkVoiceDetected;
    public int step = -1;
    public float slidingWindowTime;
    public float beforeInferTime = -1f;
    public float afterInferTime = -1f; // update time
    public float finishTime; // finish time
    public string segment = null;
    public int dup = 0;
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

    private const string log_path_prefix = "Assets\\Log\\";

    void Start()
    {
        STTManager.Instance.OnCreateWhisperStream += Init;
    }

    private void Init(WhisperStream whisperStream)
    {
        whisperStream = STTManager.Instance._stream;
        if (whisperStream == null)
        {
            UnityEngine.Debug.LogWarning("No whisper stream");
        }
        else
        {
            whisperStream.RecordAddToStream = (id) =>
            {
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
            whisperStream.RecordUseVad = (id, useVad) =>
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
            whisperStream.RecordChunkVoiceDetected = (id, value) =>
            {
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    item.chunkVoiceDetected = value;
                }
            };
            whisperStream.RecordStep = (id, step) =>
            {
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    if (item.step != -1)
                    {
                        item.dup += 1;
                    }
                    item.step = step;
                }
            };
            whisperStream.RecordSlidingWindow = (id) =>
            {
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    if (item.slidingWindowTime != -1)
                    {
                        item.dup += 1;
                    }
                    item.slidingWindowTime = Time.realtimeSinceStartup - _startTime;
                }
            };
            whisperStream.RecordBeforeInfer = (id) =>
            {

                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    if (item.beforeInferTime != -1)
                    {
                        item.dup += 1;
                    }
                    item.beforeInferTime = Time.realtimeSinceStartup - _startTime;
                }
            };
            whisperStream.RecordAfterInfer = (id) =>
            {
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    if (item.afterInferTime != -1)
                    {
                        item.dup += 1;
                    }
                    item.afterInferTime = Time.realtimeSinceStartup - _startTime;
                }
            };
            whisperStream.RecordFinished = (id) =>
            {
                LogTimelineItem item = timeline.Find((v) =>
                 {
                     return v.id == id;
                 });
                if (item != null)
                {
                    item.finishTime = Time.realtimeSinceStartup - _startTime;
                }
            };
            whisperStream.RecordSegment = (id, seg) =>
            {
                LogTimelineItem item = timeline.Find((v) =>
                {
                    return v.id == id;
                });
                if (item != null)
                {
                    if (item.segment != null)
                    {
                        item.dup += 1;
                    }
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
        FileLogUtils.OverwriteRange(timeline, "chunk/" + filename);
        timeline.Clear();
        WriteTimeline();
    }

    public void WriteTimeline()
    {
        StringBuilder sb = new StringBuilder();
        foreach (bool v in MotionGenerator.Instance.llmTimeline)
        {
            sb.Append(v ? "+" : "-");
        }
        sb.Append("\n");
        foreach (bool v in MotionGenerator.Instance.sbertTimeline)
        {
            sb.Append(v ? "+" : "-");
        }
        sb.Append("\n");
        foreach (bool v in MotionGenerator.Instance.motionTimeline)
        {
            sb.Append(v ? "+" : "-");
        }
        sb.Append("\n");

        string pull_path = Path.Combine(log_path_prefix, "timeline/"+filename + ".txt");
        // Debug.Log("Log Path: " + pull_path);

        // if (!Directory.Exists(pull_path))
        // {
        //     string dir = Path.GetDirectoryName(pull_path);
        //     Directory.CreateDirectory(dir);
        // }

        if (!File.Exists(pull_path))
        {
            // Debug.Log("Created new log file : " + pull_path);
            File.WriteAllText(pull_path, sb.ToString());
            return;
        }
    }
}

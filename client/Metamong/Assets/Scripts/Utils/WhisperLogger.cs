using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using Whisper;
using System.Linq;
using Newtonsoft.Json;
using Whisper.Samples;

// public class LogJson
// {  
//     public float stepSec;
//     public float keepSec;
//     public float lengthSec;
//     public double inferenceAvg;    
//     public List<ContextItem> context = new List<ContextItem>();

//     public class ContextItem
//     {
//         public string segment;
//         public string actionClipName;
//         public string faceClipName;
//         public double time;
//         public ContextItem(double time, FileLogVO segmentMotion){
//             this.time = time;
//             if(segmentMotion == null){
//                 return;
//             }
//             this.segment = segmentMotion.segment;
//             this.actionClipName = segmentMotion.actionClipName;
//             this.faceClipName = segmentMotion.faceClipName;
//         }
//     }

//     public void AddSegment(double time, FileLogVO segmentMotion){
//         context.Add(new ContextItem(time, segmentMotion));
//     }
// }


public class WhisperLogger : MonobehaviourSingleton<WhisperLogger>
{
    // private WhisperWrapper whisperWrapper;
    public Button button;
    public WhisperManager whisperManager;
    public FileLogVO log;

    protected override void Awake()
    {
        base.Awake();
         button.onClick.AddListener(WriteFile);
    }

    private readonly string[] targetFieldNames = { "stepSec", "keepSec", "lengthSec"};
    [SerializeField]
    private const string logPath = "whisper_log.json";
    private WhisperWrapper wrapper;
    private List<long> infer = new List<long>();

    private void WriteFile()
    {
        // Debug.Log("Log Start");
        // wrapper = whisperManager.GetWhisperWrapper();
        // if(wrapper == null){
        //     throw new Exception("다시 해 : " + wrapper);
        // }
        
        // Debug.Log("Writing to file...  ");
        
        // if (whisperManager == null)
        // {
        //     Debug.Log("⚠ targetObject가 비어있습니다!");
        //     return;
        // }

        // WhisperManager wm = whisperManager.GetComponent<WhisperManager>();
        
        // if(wm == null)
        // {
        //     Debug.Log("⚠ whisperManager 컴포넌트를 찾을 수 없습니다!");
        //     return;
        // }

        // // Component comp = targetObject.GetComponent(wmType);
        // // StringBuilder sb = new StringBuilder();

        // Type type = wm.GetType();

        // // log = new LogJson();

        // foreach (string fieldName in targetFieldNames)
        // {
        //     FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //     if (field != null)
        //     {
        //         if(field.Name == "stepSec"){
        //             log.stepSec = float.Parse(field.GetValue(wm).ToString());
        //         }

        //         if(field.Name == "keepSec"){
        //             log.keepSec = float.Parse(field.GetValue(wm).ToString());
        //         }

        //         if(field.Name == "lengthSec"){
        //         log.lengthSec = float.Parse(field.GetValue(wm).ToString());
        //         }
        //         // sb.AppendLine($"{fieldName} = {value}");
        //     }
        //     else
        //     {
        //         Debug.LogWarning($"⚠ 필드 {fieldName} 를 찾을 수 없습니다!");
        //     }
        // }
        
        // infer = wrapper.GetInferenceTime();
        // Debug.Log(string.Join(",", infer));

        // double avg = infer.Average();
        // log.inferenceAvg = Math.Round(avg, 6);
        
        // // result = sb.ToString() + $" avg = {avg:F2} \n";

        // // List<ContextItem> contexts = new List<ContextItem>();
        // // Debug.Log(stm.segment_launch.Count);
        // // Debug.Log(stm.finSegTime.Count);

        // // int maxCount = Math.Max(stm.segment_launch.Count, stm.finSegTime.Count);
        // List<FileLogVO> segmentMotionSets = STTManager.Instance.SegmentMotionSets;
        // List<double> times = STTManager.Instance.SegmentFinshiedTimes;
        // Debug.Log($"segMotionSets: {segmentMotionSets.Count}, times: {times.Count}");
        // int maxCount = segmentMotionSets.Count;
    
        // for(int i = 0; i < maxCount; i++ ){
        //     double time = i < times.Count ? times[i] : 0.0;
        //     log.AddSegment(time, segmentMotionSets[i]);
        // }

        // string source = File.ReadAllText(logPath);
        // Debug.Log("source: " + source);
        
        // List<LogJson> org =JsonConverter<LogJson>.DeserializeToList(source);

        // Debug.Log(string.Join(",", org));
        // org.Add(log);


        // File.WriteAllText(logPath, JsonConvert.SerializeObject(org, Formatting.Indented));
        
    }
}
    


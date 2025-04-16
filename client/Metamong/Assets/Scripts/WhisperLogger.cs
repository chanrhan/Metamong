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

public class LogJson
{  
    public float stepSec;
    public float keepSec;
    public float lengthSec;
    public double inferenceAvg;    
    public List<ContextItem> context = new List<ContextItem>();

    public class ContextItem
    {
        SegmentMotionSet segmentMotion;
        public double time;
        public ContextItem(double time, SegmentMotionSet segmentMotion){
            this.time = time;
            this.segmentMotion = segmentMotion;
        }
    }

    public void AddSegment(double time, SegmentMotionSet segmentMotion){
        context.Add(new ContextItem(time, segmentMotion));
    }
}


public class WhisperLogger : MonobehaviourSingleton<WhisperLogger>
{
    // private WhisperWrapper whisperWrapper;
    public Text text;
    public Button button;
    public int count = 0;
    public WhisperManager whisperManager;
    StreamingSampleMic stm;
    public LogJson log;

    protected override void Awake()
    {
        base.Awake();
        stm = GameObject.Find("DemoMic").GetComponent<StreamingSampleMic>();
    }

    private readonly string[] targetFieldNames = { "stepSec", "keepSec", "lengthSec"};
    [SerializeField]
    private const string logPath = "whisper_log.json";
    private WhisperWrapper wrapper;
    private List<long> infer = new List<long>();

    void Start()
    { 
        button.onClick.AddListener(WriteFile);
    }

    void OnDestroy()
    {
        WriteFile();
    }

    private void WriteFile()
    {
        wrapper = whisperManager.GetWhisperWrapper();
        if(wrapper == null){
            throw new Exception("다시 해 : " + wrapper);
        }
        
        if(count == 1)
        {
            Debug.Log("Writing to file...  ");
            string result = text.text;
            
            if (whisperManager == null)
            {
                Debug.LogWarning("⚠ targetObject가 비어있습니다!");
                return;
            }

            WhisperManager wm = whisperManager.GetComponent<WhisperManager>();
            
            if(wm == null)
            {
                Debug.LogWarning("⚠ whisperManager 컴포넌트를 찾을 수 없습니다!");
                return;
            }

            // Component comp = targetObject.GetComponent(wmType);
            // StringBuilder sb = new StringBuilder();

            Type type = wm.GetType();

            log = new LogJson();


            foreach (string fieldName in targetFieldNames)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    if(field.Name == "stepSec"){
                        log.stepSec = float.Parse(field.GetValue(wm).ToString());
                    }

                     if(field.Name == "keepSec"){
                        log.keepSec = float.Parse(field.GetValue(wm).ToString());
                    }

                     if(field.Name == "lengthSec"){
                        log.lengthSec = float.Parse(field.GetValue(wm).ToString());
                     }
                    // sb.AppendLine($"{fieldName} = {value}");
                }
                else
                {
                    Debug.LogWarning($"⚠ 필드 {fieldName} 를 찾을 수 없습니다!");
                }
            }
            
            
            infer = wrapper.GetInferenceTime();
            Debug.Log(string.Join(",", infer));

            double avg = infer.Average();
            log.inferenceAvg = Math.Round(avg, 6);
            
            // result = sb.ToString() + $" avg = {avg:F2} \n";

            // List<ContextItem> contexts = new List<ContextItem>();
            // Debug.Log(stm.segment_launch.Count);
            // Debug.Log(stm.finSegTime.Count);

            // int maxCount = Math.Max(stm.segment_launch.Count, stm.finSegTime.Count);
            List<SegmentMotionSet> segmentMotionSets = STTManager.Instance.SegmentMotionSets;
            List<double> times = STTManager.Instance.SegmentFinshiedTimes;
            int maxCount = segmentMotionSets.Count;
        

            for(int i = 0; i < maxCount; i++ ){
                // string seg = i < stm.segment_launch.Count ? stm.segment_launch[i] : "";
                // double ti = i < stm.finSegTime.Count ? stm.finSegTime[i] : 0.0;
                // time = i < stm.finSegTime.Count ? stm.finSegTime[i] : 0.0;

                // contexts.Add(new ContextItem{segment = seg, time = ti});
                log.AddSegment(times[i], segmentMotionSets[i]);
            }

            // log.context = contexts;

            string source = File.ReadAllText(logPath);
            Debug.Log("source: " + source);
            
            List<LogJson> org =JsonConverter<LogJson>.DeserializeToList(source);

            Debug.Log(string.Join(",", org));
            org.Add(log);


            File.WriteAllText(logPath, JsonConvert.SerializeObject(org, Formatting.Indented));
        }
        count++;
    }
}
    


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileLogVO
{
    public float stepSec;
    public float keepSec;
    public float lengthSec;
    public List<LogContextItem> logContextItems = new List<LogContextItem>();

    public class LogContextItem
    {
        public string segment;
        public List<string> whileIgnored = new List<string>();
        public long whisperInferTime;
        public long whisperFinishedInferTime;
        public long llmTime;
        public string llmResponse;
        public string emotion;
        public bool llmTimeout = false;
        public long sbertTime;
        public string actionClipName;
        public string faceClipName;
        public double actionScore;
        public double faceScore;
        public long totalElapsedTime;
        public override string ToString()
        {
            return "seg: " + segment + ", action: " + actionClipName + ", face: " + faceClipName;
        }
    }
}
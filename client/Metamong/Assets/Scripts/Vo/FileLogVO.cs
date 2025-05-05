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
        public List<string> ignored = new List<string>();
        public long inferTime;
        public long llmTime;
        public string llmResponse;
        public bool llmTimeout = false;
        public long sbertTime;
        public string actionClipName;
        public string faceClipName;
        public long totalTime;
        public override string ToString()
        {
            return "seg: " + segment + ", action: " + actionClipName + ", face: " + faceClipName;
        }
    }
}

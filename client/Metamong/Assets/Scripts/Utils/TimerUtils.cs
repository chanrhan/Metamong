using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TimerUtils
{
    private static Stopwatch sw = new Stopwatch();

    public static void Start()
    {
        sw.Start();
        UnityEngine.Debug.Log($"[chan] Timer Start");
    }
    public static void Stop()
    {
        sw.Stop();
    }
    public static void Reset()
    {
        sw.Reset();
    }
    public static void Log()
    {
        UnityEngine.Debug.Log($"[chan] Timer: {sw.ElapsedMilliseconds} ms");
    }
    public static void LogAndReset()
    {
        Log();
        Reset();
        Stop();
    }
}

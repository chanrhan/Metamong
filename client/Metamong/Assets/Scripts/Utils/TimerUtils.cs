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
    public static long Log()
    {
        UnityEngine.Debug.Log($"[chan] Timer: {sw.ElapsedMilliseconds} ms");
        return sw.ElapsedMilliseconds;
    }
    public static long LogAndReset()
    {
        long time = Log();
        Reset();
        Stop();
        return time;
    }
}

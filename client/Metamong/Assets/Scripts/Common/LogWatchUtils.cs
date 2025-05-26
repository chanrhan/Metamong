using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LogWatchUtils : MonobehaviourSingleton<LogWatchUtils>
{
    private Stopwatch stopwatch;

    public void LogStart()
    {
        stopwatch.Restart();
    }

    public void Write()
    {
        
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Whisper.Utils
{
    public enum LogLevel
    {
        Verbose,
        Log,
        Warning,
        Error,
        MyLog
    }

    /// <summary>
    /// Wrapper for Unity logger that can be configured by log level.
    /// </summary>
    public static class LogUtils
    {
        public static LogLevel Level = LogLevel.Verbose;
        private static List<string> logs = new List<string>(2000);

        private static bool isWriting = false;

        public static void Exception(Exception msg)
        {
            Debug.LogException(msg);
        }
        
        public static void Error(string msg)
        {
            Debug.LogError(msg);
        }

        public static void Warning(string msg)
        {
            if (Level > LogLevel.Warning)
                return;
            Debug.LogWarning(msg);
        }

        public static void Log(string msg)
        {
            if (Level > LogLevel.Log)
                return;
            Debug.Log(msg);
        }
        
        public static void Verbose(string msg)
        {
            if (Level > LogLevel.Verbose)
                return;
            Debug.Log(msg);
        }
        public static void MyLog(string msg)
        {
            if (isWriting)
            {
                return;
            }
            if (Level > LogLevel.MyLog)
                return;
            Debug.Log(msg);
            logs.Add($"{msg}\n");
        }

        public async static void ExitAndSaveLogFile()
        {
            isWriting = true;
            var snapshot = new List<string>(logs);

            await File.WriteAllTextAsync("log.txt", string.Join("", snapshot));
            logs.Clear();
        }
    }
}
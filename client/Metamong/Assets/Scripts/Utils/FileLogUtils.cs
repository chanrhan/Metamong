using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileLogUtils
{
    private const string log_path_prefix = "Assets\\Log\\";
    public static void Overwrite<T>(T item, string filename)
    {
        List<T> readFile = new List<T>();
        string pull_path = Path.Combine(log_path_prefix, filename + ".json");
        Debug.Log("Log Path: " + pull_path);

        // if (!Directory.Exists(pull_path))
        // {
        //     string dir = Path.GetDirectoryName(pull_path);
        //     Directory.CreateDirectory(dir);
        // }

        if (!File.Exists(pull_path))
        {
            Debug.Log("Created new log file : " + pull_path);
            readFile.Add(item);
            File.WriteAllText(pull_path, JsonConvert.SerializeObject(readFile, Formatting.Indented));
            return;
        }

        string source = File.ReadAllText(pull_path);
        // Debug.Log("source: " + source);

        readFile = JsonConverter.DeserializeToList<T>(source) ?? new List<T>();

        // Debug.Log(string.Join(",", readFile));
        readFile.Add(item);

        File.WriteAllText(pull_path, JsonConvert.SerializeObject(readFile, Formatting.Indented));
    }
}

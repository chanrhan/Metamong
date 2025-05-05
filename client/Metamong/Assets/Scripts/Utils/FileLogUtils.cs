using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileLogUtils
{
    public static void Overwrite<T>(T item, string path)
    {
        string source = File.ReadAllText(path);
        Debug.Log("source: " + source);

        List<T> readFile = JsonConverter.DeserializeToList<T>(source);

        Debug.Log(string.Join(",", readFile));
        readFile.Add(item);

        File.WriteAllText(path, JsonConvert.SerializeObject(readFile, Formatting.Indented));
    }
}

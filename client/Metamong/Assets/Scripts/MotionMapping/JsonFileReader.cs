using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

public static class JsonFileReader
{
    /// <summary>
    /// 지정된 경로의 JSON 파일을 읽어 Dictionary&lt;string, string&gt; 형태로 반환한다.
    /// JSON 파일은 { "설명": "모션파일이름", ... } 형태여야 한다.
    /// </summary>
    /// <param name="filePath">읽어올 JSON 파일의 절대 경로</param>
    /// <returns>Dictionary&lt;string, string&gt; 객체 (파일을 찾지 못하면 빈 딕셔너리 반환)</returns>
    public static Dictionary<string, string> Read(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"\"{filePath}\" 파일을 찾을 수 없습니다!");
            return new Dictionary<string, string>();
        }

        try
        {
            string json = File.ReadAllText(filePath, Encoding.UTF8);
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return dict;
        }
        catch (Exception ex)
        {
            Debug.LogError($"파일 읽기 중 오류 발생: {ex}");
            return new Dictionary<string, string>();
        }
    }
}

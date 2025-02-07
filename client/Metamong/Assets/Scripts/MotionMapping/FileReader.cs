using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

/// <summary>
/// 파일 읽어오는 전역 객체
/// </summary>
public static class FileReader{
    /// <summary>
    /// 경로에 있는 파일을 읽어오는 함수
    /// 줄바꿈(\n) 단위로 라인(Line)들을 List<string> 형태로 반환한다.
    /// </summary>
    /// <param name="filePath">filePath: 읽어올 파일의 절대 경로</param>
    public static List<string> Read(string filePath){
        List<string> list = null;
        if(File.Exists(filePath)){
            list = new List<string>();
            using(var reader = new StreamReader(filePath, Encoding.UTF8)){
                while(!reader.EndOfStream){ 
                    list.Add(reader.ReadLine());
                }
            }
        }else{
            Debug.Log($"\"{filePath}\" 파일을 찾을 수 없습니다!");
        }
        return list;
    }
}
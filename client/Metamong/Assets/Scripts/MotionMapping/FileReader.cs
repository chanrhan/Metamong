using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class FileReader{
    private readonly string filePath;
    public FileReader(string filePath){
        this.filePath = filePath;
    }

    public List<string> Read(){
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
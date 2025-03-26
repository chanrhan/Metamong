using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using System;
using System.Text;
using System.Reflection;


public class WriteFileTest : MonoBehaviour
{
    public Text text;
    public Button button;

    public int count = 0;

    public GameObject targetObject; // 대상 GameObject를 Inspector에서 지정
    public string componentTypeName = "WhisperManager"; // 컴포넌트 클래스 이름

    private readonly string[] targetFieldNames = { "stepSec", "keepSec", "lengthSec" };

    void Start()
    {
         
       
        button.onClick.AddListener(WriteFile);
    }
    private void WriteFile()
    {
        if(count == 1)
        {
            Debug.Log("Writing to file...  ");
            string result = text.text;
            if (targetObject == null)
            {
                Debug.LogWarning("⚠ targetObject가 비어있습니다!");
                return;
            }

            Component comp = targetObject.GetComponent(componentTypeName);
            StringBuilder sb = new StringBuilder();
            
            if (comp == null)
            {
                Debug.LogWarning($"⚠ 컴포넌트 '{componentTypeName}' 을(를) 찾을 수 없습니다!");
                return;
            }

            Type type = comp.GetType();

            foreach (string fieldName in targetFieldNames)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    object value = field.GetValue(comp);
                    sb.AppendLine($"{fieldName} = {value}");
                    
                }
                else
                {
                    Debug.LogWarning($"⚠ 필드 {fieldName} 를 찾을 수 없습니다!");
                }
            }
            sb.AppendLine(result);
            
            result = sb.ToString();
            File.WriteAllTextAsync("full_sentence.txt", result);
        }
        count++;
    }
}

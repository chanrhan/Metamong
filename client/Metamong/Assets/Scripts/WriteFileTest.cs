using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using System;
using System.Text;
using System.Reflection;
using Whisper;
using System.Threading.Tasks;
using System.Linq;


    
    public class WriteFileTest : MonoBehaviour
    {
        // private WhisperWrapper whisperWrapper;
        public Text text;
        public Button button;

        public int count = 0;

        public GameObject targetObject;


        private readonly string[] targetFieldNames = { "stepSec", "keepSec", "lengthSec" };

        private WhisperWrapper wrapper;
        private List<long> infer = new List<long>();
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

                WhisperManager wm = targetObject.GetComponent<WhisperManager>();
                
                if(wm == null)
                {
                    Debug.LogWarning("⚠ whisperManager 컴포넌트를 찾을 수 없습니다!");
                    return;
                }

                // Component comp = targetObject.GetComponent(wmType);
                // StringBuilder sb = new StringBuilder();

                Type type = wm.GetType();
                StringBuilder sb = new StringBuilder();

                foreach (string fieldName in targetFieldNames)
                {
                    FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        object value = field.GetValue(wm);
                        sb.AppendLine($"{fieldName} = {value}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠ 필드 {fieldName} 를 찾을 수 없습니다!");
                    }
                }
            
                sb.AppendLine(result);
                
                
                infer = wrapper.GetInferenceTime();
                Debug.Log(string.Join(",", infer));

                double avg = infer.Average();
                
                result = sb.ToString() + $"\n avg = {avg:F2}";

                File.AppendAllText("full_sentence.txt", result);
                
            }
            count++;
        }
    }
    


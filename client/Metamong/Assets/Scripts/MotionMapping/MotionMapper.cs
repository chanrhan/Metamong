 using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using System.Linq;
using UnityEngine.UI;
using System.Text;

public class MotionMapper : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputText;
    [SerializeField]
    private TextMeshProUGUI textarea;
    private List<string> keywordSentences = null;
    private TextEmbedding textEmbedding = new MLNET();
    
    
    void Awake()
    {

        FileReader reader = new FileReader("/Users/chan/Metamong/client/Metamong/Assets/Scripts/MotionMapping/keywords.txt");
        // FileReader reader = new FileReader(".keywords.txt");
        keywordSentences = reader.Read();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)){
            List<KeyValuePair<string, double>> cosineSimilarities = GetCosineSimilarities();
            
            // Rank(10)
            cosineSimilarities.Sort((v1,v2)=> v2.Value.CompareTo(v1.Value));
            StringBuilder sb = new StringBuilder();
            foreach(KeyValuePair<string, double> cos in cosineSimilarities){
                sb.Append($"{cos.Key, -60}{cos.Value.ToString("F2"), 10}\n");
                Debug.Log($"{cos.Key, -60}{cos.Value.ToString("F2"), 10}\n");
            }
            textarea.SetText(sb);
        }
    }


    private List<KeyValuePair<string, double>> GetCosineSimilarities(){
        string target = inputText.text;

        List<KeyValuePair<string, double>> list = new List<KeyValuePair<string, double>>();

        for(int i=0;i<keywordSentences.Count;++i){
            string keyword = keywordSentences[i];
            double cos = textEmbedding.CompareWordText(target, keyword);
            // Debug.Log($"{target}-{keyword} : {cos}");
            list.Add(new KeyValuePair<string, double>(keyword, cos));
        }

        return list;
    }
}
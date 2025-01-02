using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Runtime;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using Tensorflow;
using Unity.VisualScripting.Dependencies.Sqlite;
using Word2vec;
using Word2vec.Tools;
using System.Data.Odbc;

public class MotionMapper : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputText;
     [SerializeField]
    private TextMeshProUGUI outputText;
    [SerializeField]
    private int vectorSize = 10;
    [SerializeField]
    private string[] exampleKeywords = new string[9];

    private ITextEmbeddable embeddingModel;
    
    void Start()
    {
        GameObject pannel = GameObject.Find("ExampleKeywords");
        TextMeshProUGUI[] textMeshProUGUIs = pannel.GetComponentsInChildren<TextMeshProUGUI>();
        for(int i=0;i<textMeshProUGUIs.Length;++i){
            textMeshProUGUIs[i].SetText(exampleKeywords[i]);
        }

        // word2vec 사용 
        embeddingModel = new Word2VecModel(vectorSize);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)){
            // Debug.Log("enter");
            startMotionMapping();
        }
    }

    // 1. 모션 키워드 가져오기
    // 2. 가져온 모션 키워드를 임베딩 벡터로 전환 
    // 3. 사전 임베딩 테이블에서 유사도 검사를 통해 최적의 키워드 산출 (코사인 유사도 검사)
    // 4. 애니메이션 테이블에서 최적 키워드를 키값으로 하는 .fbx 파일 추출 
    // 5. 캐릭터에게 애니메이션 입히기 

    // 2. 텍스트 임베딩 : 단어를 실수 벡터로 변환
    void startMotionMapping(){

        foreach(string keyword in exampleKeywords){
            double cos = embeddingModel.GetCosinSimilarity(inputText.text, keyword);
            Debug.Log($"{inputText.text}={keyword} : {cos}");
        }
    }

   

    

    

    
    

    void UseSBERT(){
        // Create Tokenizer and tokenize the sentence.
        // var tokenizer = new BertUncasedLargeTokenizer();
        // var tokenizer = new BERTWrapper();

        // // Get the sentence tokens.
        // var tokens = tokenizer.Tokenize(sentence);
        // Console.WriteLine(String.Join(", ", tokens));

        // // Encode the sentence and pass in the count of the tokens in the sentence.
        // var encoded = tokenizer.Encode(tokens.Count(), sentence);
        

        // // Break out encoding to InputIds, AttentionMask and TypeIds from list of (input_id, attention_mask, type_id).
        // var bertInput = new BertInput()
        // {
        //     InputIds = encoded.Select(t => t.InputIds).ToArray(),
        //     AttentionMask = encoded.Select(t => t.AttentionMask).ToArray(),
        //     TypeIds = encoded.Select(t => t.TokenTypeIds).ToArray(),
        // };
    }
}
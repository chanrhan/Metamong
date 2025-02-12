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

/// <summary>
/// 모션 매핑을 수행하는 주 클래스
/// </summary>
public class MotionMapper : MonoBehaviour
{
    /// <summary>
    /// 문자열을 입력받기 위한 필드 
    /// </summary>
    [SerializeField]
    private TMP_InputField inputText; 
    /// <summary>
    /// 최종 결과값을 표시해주는 TextArea
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI textarea;
    private List<string> testSentenceList = null;

    // 사용할 임베딩 모델을 명시 
    private TextEmbedding textEmbedding = new MLNET();
    
    // Application.dataPath는 현재 Assets폴더를 반환한다
    // TEST_SENTENCES.txt 파일은 유사도 검사가 잘 이루어지를 테스트하기 위해 임의로 만든 파일임 
    // 해당 파일을 임의로 수정하여 테스트해보면 됨 
    private static string TEST_SENTENCES_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/TEST_SENTENCES.txt";
    
    void Awake()
    {
        // 테스트 문장 있는 파일 읽어오기 
        testSentenceList = FileReader.Read(TEST_SENTENCES_FILE_PATH);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)){ // Enter 키 입력 시 실행 
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

    /// <summary>
    /// 현재 InputField 에 입력된 문자열과 TEST_SENTENCES 내의 문자열들을 코사인 유사도 검사를 통해 비교
    /// </summary>
    /// <returns>현재 InputField에 입력된 문자열과 비교 대상이 되는 문자열(string)과 해당 문자열과의 코사인 유사도 수치(double) 들의 배열</returns>
    private List<KeyValuePair<string, double>> GetCosineSimilarities(){
        string target = inputText.text; // 현재 InputField에 입력된 문자열 

        // 문자열(string), 유사도 수치(double) 를 KeyValuePair로 묶어 결과값을 담는 배열 생성 
        List<KeyValuePair<string, double>> list = new List<KeyValuePair<string, double>>();

        for(int i=0;i<testSentenceList.Count;++i){
            string keyword = testSentenceList[i];
            double cos = textEmbedding.CompareWordText(target, keyword); 
            // Debug.Log($"{target}-{keyword} : {cos}"); // 디버그용 콘솔 로그 
            list.Add(new KeyValuePair<string, double>(keyword, cos));
        }

        return list;
    }
}
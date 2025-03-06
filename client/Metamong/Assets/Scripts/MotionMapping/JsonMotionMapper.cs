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
using UnityEngine.Rendering;
using Unity.VisualScripting;

/// <summary>
/// 모션 매핑을 수행하는 주 클래스
/// </summary>
public class JsonMotionMapper : MonoBehaviour
{
    private string actionText;

        void OnEnable()
    {
        // ChatCompletion의 이벤트 구독
        ChatCompletionWithSummary.OnActionTextUpdated += UpdateActionText;
    }

    void OnDisable()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        ChatCompletionWithSummary.OnActionTextUpdated -= UpdateActionText;
    }
private void UpdateActionText(string newText)
{
    actionText = newText;
    float threshold = 0.4f;

    string[] motionText = actionText.Split('\n');

    // 새로 만든 GetMotionKeys 함수 호출: List<string> 반환
    List<string> motionKeys = GetMotionKeys(motionText, threshold);

    // 각 모션 입력에 대한 결과를 라벨과 함께 출력 (순서대로 UserAct, UserFace, NPCAct, NPCFace)
    string[] motionLabels = new string[] { "UserAct", "UserFace", "NPCAct", "NPCFace" };

    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < motionKeys.Count; i++)
    {
        // 빈 문자열인 경우 "No match"로 표시
        string key = string.IsNullOrEmpty(motionKeys[i]) ? "No match" : motionKeys[i];
        sb.AppendLine($"{motionLabels[i], -10}: {key}");
        Debug.Log($"{motionLabels[i], -10}: {key}");
    }

    textarea.SetText(sb.ToString());
}
    /// <summary>
    /// 최종 결과값을 표시해주는 TextArea
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI textarea;
    private Dictionary<string, string> actMotionList = null;
    private Dictionary<string, string> faceMotionList = null;

    // 사용할 임베딩 모델을 명시 
    private TextEmbedding textEmbedding = new MLNET();
    
    // Application.dataPath는 현재 Assets폴더를 반환한다
    // TEST_SENTENCES.txt 파일은 유사도 검사가 잘 이루어지를 테스트하기 위해 임의로 만든 파일임 
    // 해당 파일을 임의로 수정하여 테스트해보면 됨 
    private static string ACTMOTION_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/ActMotion.json";
    private static string FACEMOTION_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/FaceMotion.json";


void Awake()
{
    actMotionList = JsonFileReader.Read(ACTMOTION_FILE_PATH);
    faceMotionList = JsonFileReader.Read(FACEMOTION_FILE_PATH);
}
/// <summary>
/// 입력된 각 모션 텍스트에 대해 해당 카테고리(act 혹은 face)의 모션 리스트에서 SBERT 유사도 검사를 수행한다.
/// 유사도가 threshold 이상인 경우 해당 모션의 파일 이름을 반환하며, 그렇지 않으면 빈 문자열("")을 반환한다.
/// motions 배열 순서: 0 - UserAct, 1 - UserFace, 2 - NPCAct, 3 - NPCFace
/// </summary>
/// <param name="motions">사용자 입력 모션 텍스트 배열</
// 
// param>
/// <param name="threshold">유사도 스레시홀드 값 (예: 0.5)</param>
/// <returns>각 입력에 대한 결과 모션 파일 이름의 리스트 (유사도가 threshold 미만인 경우 빈 문자열)</returns>
private List<string>  GetMotionKeys(string[] motions, double threshold)
    {
    List<string> results = new List<string>();

    for (int idx = 0; idx < motions.Length; idx++)
    {
        string inputText = motions[idx];
        // Act 모션은 인덱스 0 (UserAct)와 2 (NPCAct), Face 모션은 인덱스 1 (UserFace)와 3 (NPCFace)
        Dictionary<string, string> dict = (idx == 0 || idx == 2) ? actMotionList : faceMotionList;

        double bestScore = -1.0;
        string bestMotionKey = string.Empty;

        // 각 후보(키: 모션 설명, 값: 모션 파일 이름)를 순회하며 SBERT 유사도 검사
        foreach (KeyValuePair<string, string> kvp in dict)
        {
            double score = textEmbedding.CompareWordText(inputText, kvp.Key);
            if (score > bestScore)
            {
                bestScore = score;
                bestMotionKey = kvp.Value;
            }
        }

            // 스레시홀드 이상인 경우 모션 키를 결과에 추가, 아니면 빈 문자열("") 추가
            if (bestScore >= threshold)
            {
                results.Add(bestMotionKey);
            }
            else
            {
                results.Add(string.Empty);
            }
        }

        return results;
    }   
}
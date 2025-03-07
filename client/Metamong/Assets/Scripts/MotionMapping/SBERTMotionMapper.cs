using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class SBERTMotionMapper : MonoBehaviour
{
    private string actionText;

    // ChatCompletion의 이벤트 구독 (프로젝트에 맞게 이벤트 이름과 처리 방식을 수정)
    void OnEnable()
    {
        ChatCompletionWithSummary.OnActionTextUpdated += UpdateActionText;
    }

    void OnDisable()
    {
        ChatCompletionWithSummary.OnActionTextUpdated -= UpdateActionText;
    }

    /// <summary>
    /// 사용자 입력 텍스트가 업데이트될 때 호출됩니다.
    /// 입력된 각 줄(모션 텍스트)에 대해 SBERT 임베딩 기반 유사도 검사를 수행하고,
    /// 스레시홀드 이상이면 해당 모션 키를, 아니면 "No match"를 출력합니다.
    /// </summary>
    /// <param name="newText">사용자 입력 텍스트 (각 줄이 하나의 모션 텍스트)</param>
    private void UpdateActionText(string newText)
    {
        actionText = newText;
        float threshold = 0.4f;
        string[] motionText = actionText.Split('\n');

        // 모션 키 리스트 추출
        List<string> motionKeys = GetMotionKeys(motionText, threshold);

        // 모션 라벨 (순서대로: UserAct, UserFace, NPCAct, NPCFace)
        string[] motionLabels = new string[] { "UserAct", "UserFace", "NPCAct", "NPCFace" };

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < motionKeys.Count; i++)
        {
            string key = string.IsNullOrEmpty(motionKeys[i]) ? "No match" : motionKeys[i];
            sb.AppendLine($"{motionLabels[i], -10}: {key}");
            Debug.Log($"{motionLabels[i], -10}: {key}");
        }
        textarea.SetText(sb.ToString());
    }

    /// <summary>
    /// 결과를 표시할 TextMeshProUGUI (Inspector에서 할당)
    /// </summary>
    [SerializeField] private TextMeshProUGUI textarea;

    // 모션 JSON 파일로부터 읽어온 사전 (키: 모션 설명, 값: 모션 키)
    private Dictionary<string, string> actMotionList;
    private Dictionary<string, string> faceMotionList;

    // 사용할 SBERT 임베딩 모델 (SBERTEmbedding 컴포넌트)
    private SBERTEmbedding sbertEmbedding;

    // JSON 파일 경로 (절대 경로 예: Application.dataPath 기준)
    private static string ACTMOTION_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/ActMotion.json";
    private static string FACEMOTION_FILE_PATH = Application.dataPath + "/Scripts/MotionMapping/FaceMotion.json";

    void Awake()
    {
        // JSON 파일에서 모션 사전 읽어오기
        actMotionList = JsonFileReader.Read(ACTMOTION_FILE_PATH);
        faceMotionList = JsonFileReader.Read(FACEMOTION_FILE_PATH);

        // SBERTEmbedding 컴포넌트를 같은 GameObject에서 찾기
        sbertEmbedding = GetComponent<SBERTEmbedding>();
        if (sbertEmbedding == null)
        {
            Debug.LogError("SBERTEmbedding 컴포넌트를 찾을 수 없습니다. 해당 GameObject에 부착되어 있는지 확인하세요.");
        }
    }

    /// <summary>
    /// 입력된 각 모션 텍스트에 대해, 해당 카테고리(Act 또는 Face)의 모션 사전에서 SBERT 기반 유사도 검사를 수행합니다.
    /// 유사도가 threshold 이상이면 해당 모션 키를, 그렇지 않으면 빈 문자열("")을 반환합니다.
    /// motions 배열 순서: 0 - UserAct, 1 - UserFace, 2 - NPCAct, 3 - NPCFace
    /// </summary>
    /// <param name="motions">사용자 입력 모션 텍스트 배열</param>
    /// <param name="threshold">유사도 스레시홀드 값 (예: 0.4)</param>
    /// <returns>각 입력에 대한 결과 모션 키 리스트</returns>
    private List<string> GetMotionKeys(string[] motions, double threshold)
    {
        List<string> results = new List<string>();

        for (int idx = 0; idx < motions.Length; idx++)
        {
            string inputText = motions[idx];
            // Act 모션: 인덱스 0 (UserAct)와 2 (NPCAct)
            // Face 모션: 인덱스 1 (UserFace)와 3 (NPCFace)
            Dictionary<string, string> dict = (idx == 0 || idx == 2) ? actMotionList : faceMotionList;

            double bestScore = -1.0;
            string bestMotionKey = string.Empty;

            // 사전의 각 항목과 입력 텍스트 간 유사도 계산
            foreach (KeyValuePair<string, string> kvp in dict)
            {
                double score = sbertEmbedding.CompareWordText(inputText, kvp.Key);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMotionKey = kvp.Value;
                }
            }
            results.Add(bestScore >= threshold ? bestMotionKey : string.Empty);
        }

        return results;
    }
}

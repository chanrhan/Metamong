using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class SBERTMotionMapper : MonoBehaviour
{
    private string actionText;
    private string UserAct;
    private string UserFace;
    private string NPCAct;
    private string NPCFace;

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
        string[] motionText = actionText.Split('\n');

        // 모션 키 리스트 추출
        GetMotionKeys(motionText);

        // 모션 라벨 (순서대로: UserAct, UserFace, NPCAct, NPCFace)
        string[] motionLabels = new string[] { "UserAct", "UserFace", "NPCAct", "NPCFace" };

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{motionLabels[0], -10}: {UserAct}");
        sb.AppendLine($"{motionLabels[1], -10}: {UserFace}");
        sb.AppendLine($"{motionLabels[2], -10}: {NPCAct}");
        sb.AppendLine($"{motionLabels[3], -10}: {NPCFace}");
        textarea.SetText(sb.ToString());
    }

    /// <summary>
    /// 결과를 표시할 TextMeshProUGUI (Inspector에서 할당)
    /// </summary>
    [SerializeField] private TextMeshProUGUI textarea;

    // 사용할 SBERT 임베딩 모델 (SBERTEmbedding 컴포넌트)
    private SBERTEmbedding sbertEmbedding;

    void Awake()
    {
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
    private void GetMotionKeys(string[] motions)
    {
        UserAct = sbertEmbedding.CompareWordText(motions[0], true);
        UserFace = sbertEmbedding.CompareWordText(motions[1], false);
        NPCAct = sbertEmbedding.CompareWordText(motions[2], true);
        NPCFace = sbertEmbedding.CompareWordText(motions[3], false);

        return;
    }
}

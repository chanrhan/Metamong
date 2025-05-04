using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

[Obsolete]
public class SBERTMotionMapper : MonoBehaviour
{
    private string actionText;

    [SerializeField]
    private PlayerController PlayerController;
    
    [SerializeField]
    private NpcAI NpcAI;
    
    // 사용할 SBERT 임베딩 모델 (SBERTEmbedding 컴포넌트)
    private SBERT sbertEmbedding;
    //ChatCompletion의 이벤트 구독 (프로젝트에 맞게 이벤트 이름과 처리 방식을 수정)

    void Awake()
    {
        // SBERTEmbedding 컴포넌트를 같은 GameObject에서 찾기
        // PlayerController = transform.parent.GetComponent<PlayerController>();
        //PlayerController.OnActionTextUpdated += UpdateActionText;
        
        // if(PlayerController == null){
        //     NpcAI = transform.parent.GetComponent<NpcAI>();
            // ChatCompletionWithSummary.NPCActionTextUpdated += NPCUpdateActionText;
        // }else
        // {
        //     PlayerController.OnActionTextUpdated += UpdateActionText;
        // }
        //ChatCompletionWithSummary.OnActionTextUpdated += UpdateActionText;

        sbertEmbedding = GetComponent<SBERT>();

        if (sbertEmbedding == null)
        {
            Debug.LogError("SBERTEmbedding 컴포넌트를 찾을 수 없습니다. 해당 GameObject에 부착되어 있는지 확인하세요.");
        }
    }

    /// <summary>
    /// 사용자 입력 텍스트가 업데이트될 때 호출됩니다.
    /// 입력된 각 줄(모션 텍스트)에 대해 SBERT 임베딩 기반 유사도 검사를 수행하고,
    /// 스레시홀드 이상이면 해당 모션 키를, 아니면 "No match"를 출력합니다.
    /// </summary>
    /// <param name="newText">사용자 입력 텍스트 (각 줄이 하나의 모션 텍스트)</param>
    // private void UpdateActionText(string newText, SegmentMotionSet segmentMotionSet = default)
    // {
    //     actionText = newText;
    //     string[] keywords = GetMotionKeys(newText);
    //     // segmentMotionSet.actionClipName = keywords[0];
    //     // segmentMotionSet.faceClipName = keywords[1];

    //     // PlayerController.PlayMotion(keywords[0], keywords[1]);
    // }

    // private void NPCUpdateActionText(string newText)
    // {
    //     actionText = newText;
    //     string[] keywords = GetMotionKeys(newText);

    //     NpcAI.MakeMotion(keywords[0]);
    //     NpcAI.MakeFace(keywords[1]);
    // }

    /// <summary>
    /// 입력된 각 모션 텍스트에 대해, 해당 카테고리(Act 또는 Face)의 모션 사전에서 SBERT 기반 유사도 검사를 수행합니다.
    /// 유사도가 threshold 이상이면 해당 모션 키를, 그렇지 않으면 빈 문자열("")을 반환합니다.
    /// motions 배열 순서: 0 - UserAct, 1 - UserFace, 2 - NPCAct, 3 - NPCFace
    /// </summary>
    /// <param name="motions">사용자 입력 모션 텍스트 배열</param>
    public string[] GetMotionKeys(string motions)
    {
        string[] keywords = new string[2];
        keywords[0] = sbertEmbedding.CompareWordText(motions, true);
        keywords[1] = sbertEmbedding.CompareWordText(motions, false);
    
       return keywords;
    }
}

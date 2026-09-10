using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

[Obsolete]
public class SBERTMotionMapper : MonoBehaviour
{
    [SerializeField]
    private PlayerController PlayerController;
    
    [SerializeField]
    private NpcAI NpcAI;
    
    // 사용할 SBERT 임베딩 모델 (SBERTEmbedding 컴포넌트)
    private SBERT sbertEmbedding;
    //ChatCompletion의 이벤트 구독 (프로젝트에 맞게 이벤트 이름과 처리 방식을 수정)

    void Awake()
    {
        sbertEmbedding = GetComponent<SBERT>();

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
    // public string[] GetMotionKeys(string motions)
    // {
    //     string[] keywords = new string[2];
    //     keywords[0] = sbertEmbedding.CompareWordText(motions, true);
    //     keywords[1] = sbertEmbedding.CompareWordText(motions, false);
    
    //    return keywords;
    // }
}
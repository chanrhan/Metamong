using System.Collections;
using UnityEngine;
using LLMUnity;  // LLMUnity 관련 네임스페이스

public class ActionExtractor : MonoBehaviour
{
    private LLMCharacter llmCharacter;  // LLMCharacter 인스턴스

    void Start()
    {
        LLM llm = gameObject.AddComponent<LLM>();
        llm.SetModel("Phi-3-mini-4k-instruct-q4.gguf");
        llmCharacter = gameObject.AddComponent<LLMCharacter>();
        llmCharacter.llm = llm;

        llmCharacter.SetPrompt("Extract actions from the user sentences.");
    }

    /// <summary>
    /// 유저 문장과 NPC 문장에서 행동과 표정 추출하는 함수
    /// </summary>
    /// <param name="Input">유저 입력</param>
    public void ExtractActions(string InputText)
    {
        //string extractionSystemInstruction = "";
        //_ = llmCharacter.Complete(extractionSystemInstruction + "\n" + InputText, HandleReply, ReplyCompleted);
        _ = llmCharacter.Complete(InputText, HandleReply);
    
    }

    /// <summary>
    /// LLM 모델의 응답을 처리하는 메서드
    /// </summary>
    /// <param name="reply">모델의 응답</param>
    private void HandleReply(string reply)
    {
        //extractedActionsText.text = reply.Trim();
        Debug.Log("Extracted Actions: " + reply);   
    }
}

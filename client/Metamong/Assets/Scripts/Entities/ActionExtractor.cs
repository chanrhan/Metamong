using System.Collections;
using UnityEngine;
using LLMUnity;  // LLMUnity 관련 네임스페이스

public class ActionExtractor : MonoBehaviour
{
    private LLMCharacter llmCharacter;  // LLMCharacter 인스턴스
    private LLM llm;

    // void Start()
    // {
    //     gameObject.SetActive(false);
    //     LLM llm = gameObject.AddComponent<LLM>();
    //     llm.SetModel("llama-3.2-3b-instruct-q4_k_m.gguf");
    //     llm.numThreads = -1;
    //     llm.numGPULayers = 10;

    //     llmCharacter = gameObject.AddComponent<LLMCharacter>();
    //     llmCharacter.llm = llm;
    //     // 추가 옵션 (필요 시 활성화)
    //     // llmCharacter.stream = true;         // 스트리밍 응답을 활성화
    //     // llmCharacter.save = "AICharacter1";   // 저장 경로 설정
    //     // llmCharacter.saveCache = true;        // 저장 캐시 활성화
    //     // await llmCharacter.SetGrammar("json.gbnf"); // 문법 설정
    //     llmCharacter.SetPrompt("Extract actions from the user sentences.");


    //     gameObject.SetActive(true);

    // }
    // void Awake() {
    // // 같은 GameObject에 LLMCharacter 컴포넌트가 붙어 있다면:
    // llmCharacter = GetComponent<LLMCharacter>();

    // // 만약 Inspector에서 수동으로 할당하려면 public으로 만든 후, Inspector에 할당하세요.
    // }

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

using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class LlmManager : MonobehaviourSingleton<LlmManager>
{
    //LLM llm;
    private LLMCharacter myllmCharacter;
    public LLMCharacter MyLlmCharacter {get => myllmCharacter;}
    private LLM llm;

    private Queue<string> chatHistory = new Queue<string>(10);

    protected override void Awake()
    {
            base.Awake();
            llm = GetComponent<LLM>();
            myllmCharacter = GetComponent<LLMCharacter>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        
        llm.SetModel("llama-3-Korean-Bllossom-8B-Q4_K_M.gguf");
            llm.numThreads = -1;
            llm.numGPULayers = 10;
            myllmCharacter.llm = llm;

            myllmCharacter.SetPrompt(
            "당신은 한국어 대화 분석 전문가입니다.\n" +
            "아래 대화 맥락을 분석하고, 마지막 발화에 집중하여 다음 네 가지 핵심 요소를 중심으로 분석하세요:\n" +
            "   - 대화의 주요 아이디어를 한 단어로 요약한 키워드\n" +
            "   - 내포된 감정 (예: 기쁨, 슬픔, 분노, 비꼬는, 진지함 등)\n" +
            "   - 암시되는 표정 (예: 미소, 찡그림, 비꼬는 표정 등)\n" +
            "   - 관련된 행동 또는 모션 (예: 웃음, 어깨 으쓱, 끄덕임 등)\n" +
            "대화 맥락 전체를 고려하여, 마지막 발화가 비꼬는지 혹은 진심인지를 판단한 후, 위 네 가지 요소를 모두 반영한 자연스럽고 일관된 한 문장의 영어 문장으로 요약된 결과를 출력하세요.\n" +
            "\n" +
            "예시:\n" +
            "대화 맥락:\n" +
            "    jjj : 오늘 날씨 어때?\n" +
            "    aaa : 뭐, 별로야.\n" +
            "마지막 발화: aaa : 정말? 아무리 그래도...\n" +
            "출력 예: 'A mildly skeptical remark with a slight frown and a shrug, questioning the sincerity of the comment.'\n" +
            "\n" +
            "아래 대화 맥락과 마지막 발화를 참고하여 작업을 수행하세요:" +
            "---\n"
            );
            gameObject.SetActive(true);


            // 카메라 및 플레이어 오브젝트 설정
            //CameraController.Instance.SetTargetPlayer(gameObject);
            //ClientManager.Instance.MyPlayerObject = gameObject;

    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L)){
            Debug.Log(GenerateChatLogs());
        }
    }

    public void AddChatLog(string playerId, string msg) {
        
        if(chatHistory.Count + 1 > 10)
        {
            chatHistory.Dequeue();
        }
        chatHistory.Enqueue($"{playerId}:{msg}");

    }

    public string GenerateChatLogs()
    {
        StringBuilder strBuilder = new StringBuilder("");
        foreach(string str in chatHistory)
        {
            strBuilder.Append($"{str}\n");
        }
        
        return strBuilder.ToString();
    }

    public async Task<string> Chat(string query, Callback<string> callback = null, EmptyCallback completionCallback = null, bool addToHistory = false)
    {
        string logs = GenerateChatLogs();

        return await myllmCharacter.Chat(logs + "---\n 마지막 발화: " + query, callback, completionCallback, addToHistory);
    }
}

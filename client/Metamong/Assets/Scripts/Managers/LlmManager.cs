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
            "당신은 한국어 일상 대화 분석 전문가입니다.\n" +
            "아래 대화 맥락 전체를 고려하여 위 세 가지 요소를 모두 반영한 자연스럽고 일관된 한 문장의 영어 문장으로 요약된 결과를 출력하세요.\n" +
            "   1. 대화의 주요 맥락을 한 단어로 요약한 키워드(예: 인사, 정보 요청, 분노 표출 등)\n" +
            "   2. 내포된 감정 (예: 기쁨, 슬픔, 분노, 비꼬는, 진지함 등)\n" +
            "   3. 관련된 행동 (예: 웃기, 손 흔들기, 고개를 끄덕임 등)\n" +
            "\n" +
            "출력은 다음 형식을 따라야 합니다:\n\n" +
            "Output :\n" +
            "[Few-shot Examples]\n" +
            "Example 1)\n" +
            "bbb : 나 오늘 슬픈 일 있었어\n" +
            "마지막 발화 ccc : 무슨 일 있었어\n" +
            "Output : A concerned inquiry with a sympathetic tone and a slight head tilt, asking what happened\n" +
            "Example 2)\n" +
            "yyy : 나 갑자기 연차가 생겨서 놀러갈거야 당일 여행 추천 부탁해\n" +
            "ttt : 너 혼자 가는 거임? 수원에서 멀지 않은 거리면 예산 어때\n" +
            "yyy : 어 너 거기 가봤어? 거기 어때?\n" +
            "ttt : 웅 대학교 친구들이랑 같이 갔었는데 다양한 먹거리도 많아서 너무 좋았어\n" +
            "마지막 발화 yyy : 가보고 싶긴했는데 이번기회에 가봐야겠다 수원에서 얼마나 걸려?\n" +
            "Output : A friendly information request with a curious tone and a slight head tilt, seeking clarification on the travel time from Suwon.\n" +
            "\n" +
            "아래 대화 맥락과 마지막 발화를 참고하여 작업을 수행하세요:\n" +
            "대화 맥락이 없다면 마지막 발화만 참고하여 작업을 수행하세요:\n" +
            "---\n"
            );
            gameObject.SetActive(true);
//"최종 출력 결과는 오직 하나의 영어 문장만 제공되어야 하며, 추가적인 설명이나 문구가 포함되어서는 안 됩니다.\n" +
            

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
        string response = await myllmCharacter.Chat(logs + "마지막 발화 " + query, callback, completionCallback, addToHistory);
        string prefix = "Output :";
        string result = response.StartsWith(prefix) ? response.Substring(prefix.Length).Trim() : response.Trim();

        return result;
    }
}

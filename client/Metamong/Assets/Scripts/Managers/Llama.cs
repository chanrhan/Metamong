using LLMUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Llama : MonoBehaviour
{
    [SerializeField]
    private int maxChatLogLength = 10;
    private LLMCharacter myllmCharacter;
    private LLM llm;
    public LLMCharacter MyLlmCharacter { get => myllmCharacter; }
    
    
    private Queue<string> chatHistory = new Queue<string>(10);

    void Awake()
    {
            llm = GetComponent<LLM>();
            myllmCharacter = GetComponent<LLMCharacter>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        
        // llm.SetModel("llama-3.2-Korean-Bllossom-3B-IQ3_M.gguf");
        // llm.numThreads = -1;
        // llm.numGPULayers = 30;
        myllmCharacter.llm = llm;

        myllmCharacter.SetPrompt(
        "당신은 한국어 일상 대화 분석 전문가입니다.\n" +
        "주어진 전체 대화를 참고하여, 마지막 발화를 중심으로 마지막으로 말한 이가 무엇을 하고 있는 지를 감정을 반영하여 하나의 영어 문장으로 요약하세요.\n" +
        "출력 형식: Output: <영어 문장>\n\n" +

        "예시:\n" +
        "bbb: 나 오늘 슬픈 일 있었어\n" +
        "마지막 발화 ccc: 무슨 일 있었어?\n" +
        "Output: The speaker, feeling worried, is trying to comfort the other person.\n\n" +

        "yyy: 나 갑자기 연차가 생겨서 놀러갈거야 당일 여행 추천 부탁해\n" +
        "ttt: 너 혼자 가는 거임? 수원에서 멀지 않은 거리면 예산 어때\n" +
        "yyy: 어 너 거기 가봤어? 거기 어때?\n" +
        "ttt: 대학교 친구들이랑 같이 갔었는데 먹거리도 다양해서 좋았어\n" +
        "마지막 발화 yyy: 아 그래 이번에 가봐야겠다\n" +
        "Output: The speaker, feeling satisfied, is agreeing with the other person.\n\n" +

        "마지막 발화 ggg: 아 진짜 너무 심심하다.\n" +
        "Output: The speaker, feeling bored, is talking to himself absentmindedly.\n\n" +

        "aaa: 나 오늘 기분이 너무 좋아\n" +
        "bbb: 왜? 무슨 일 있어?\n" +
        "마지막 발화 aaa: 그냥 기분이 좋아\n" +
        "Output: The speaker, feeling positive, is casually expressing happiness.\n\n" +

        "mmm: 요즘 너무 바빠서 정신이 하나도 없어\n" +
        "jjj: 일이 많아?\n" +
        "mmm: 어, 회의도 많고 보고서도 많아서 정신이 없어\n" +
        "jjj: 힘들겠다, 좀 쉬어야겠네\n" +
        "마지막 발화 mmm: 나도 쉴 수 있었으면 진작에 쉬었지\n" +
        "Output: The speaker, feeling slightly angry, is quietly lamenting his own situation.\n\n" +

        "kun: 나 오늘 학교에서 재밌는일 있었어\n" +
        "min: 뭔데?\n" +
        "kun: 학교에서 선생님이 걷다가 계단에서 넘어졌어 너무 웃겨\n" +
        "마지막 발화 min: 너는 그게 웃기니?\n" +
        "Output: The speaker, feeling disappointed, is blaming the other person.\n\n" +

        "jin: 오늘 쿵푸펜더 나왔는데 보러갈래?\n" +
        "gyu: 아 진짜? 나 그거 너무 보고싶었어\n" +
        "jin: 그럼 보러가자\n" +
        "gyu: 응, 언제 보러갈래?\n" +
        "jin: 이번 주 토요일 어때?\n" +
        "마지막 발화 gyu: 헐, 그날은 시간 안되는데\n" +
        "Output: The speaker, feeling regretful, is politely declining the other person's suggestion.\n\n" +

        "전체 대화:\n" +
        "{여기에 대화 맥락}\n\n" +
        "마지막 발화:\n" +
        "{여기에 마지막 발화}\n"
        );
        gameObject.SetActive(true);

        // 카메라 및 플레이어 오브젝트 설정
        //CameraController.Instance.SetTargetPlayer(gameObject);
        //ClientManager.Instance.MyPlayerObject = gameObject;
    }

    private void Update()
    {
        // 인게임 씬이 아니면 동작하지 않도록 
        if(SceneManager.GetActiveScene().name != GameSceneManager.Instance.InGameSceneName){
            return;
        }

        if(Input.GetKeyDown(KeyCode.L)){
            Debug.Log(GenerateChatLogs());
        }
    }

    public void AddChatLog(string playerId, string msg) {
        while(chatHistory.Count >= maxChatLogLength)
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

    public void ClearChatLogs()
    {
        chatHistory.Clear();
    }

    public async Task<string> Chat(string query, CancellationToken token, Callback<string> callback = null, EmptyCallback completionCallback = null, bool addToHistory = false)
    {
        token.ThrowIfCancellationRequested();
        string logs = GenerateChatLogs();
        Debug.Log($"[yun] {logs}마지막 발화 {query}");
        string response = await myllmCharacter.Chat(logs + "마지막 발화 " + query, callback, completionCallback, addToHistory, token);
        if (string.IsNullOrEmpty(response))
        {
            return "";
        }
        string prefix = "Output :";
        string result = response.StartsWith(prefix) ? response.Substring(prefix.Length).Trim() : response.Trim();

        return result;
    }
}

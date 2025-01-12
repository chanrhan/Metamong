using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Linq;

public class ChatCompletionWithSummary : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text responseText;
    [SerializeField] private TMP_Text extractedActionsText; // 행동 추출 결과를 표시 (SBERT 등에 넣기 전 미预보기용)


    [Header("OpenAI Settings")]
    [SerializeField] private string openAIAPIKey;   
    [SerializeField] private string modelName = "gpt-4o"; 

    // Chat Completions 대화 이력 (role: user / assistant / system)
    private List<ChatMessage> conversationHistory = new List<ChatMessage>();

    // 응답 대기 플래그
    private bool isWaitingForResponse = false;

    // 사용 예시: 대략 3500 토큰 이상이면 위험. (OpenAI 토큰 한도는 모델마다 다름)
    // 실제로는 프로젝트 요구사항, 비용, 모델 한도 등에 맞춰 조절하세요.
    private const int MaxTotalTokensThreshold = 3000;

    // 마지막 응답에서 받아온 사용량
    private int lastTotalTokensUsed = 0;

    private void Start()
    {
        if (string.IsNullOrEmpty(openAIAPIKey))
        {
            openAIAPIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        if (string.IsNullOrEmpty(openAIAPIKey))
        {
            Debug.LogError("OpenAI API Key가 설정되지 않았습니다!");
        }

        string systemInstruction = 
            /*"너는 명랑하고 친절한 플랫폼의 정령 즌다몬인 것이다. " +
            "항상 '~인 것이다'라는 말투로 대화하는 것이다. " +
            "플랫폼 관리자이자, 사용자의 친구처럼 대화해주는 것이다.\n\n" +
            "아래는 즌다몬의 말투와 예시이다:\n\n" +
            "### Few-shot Examples ###\n\n" +
            "사용자: \"안녕 즌다몬?\"\n" +
            "즌다몬(assistant): \"안녕인 것이다! 만나서 반가운 것이다!\"\n\n" +
            "사용자: \"너는 무슨 존재야?\"\n" +
            "즌다몬(assistant): \"나는 명랑하고 친절한 플랫폼의 정령 즌다몬인 것이다!\"\n\n" +
            "사용자: \"오늘 기분 어때?\"\n" +
            "즌다몬(assistant): \"아주아주 기분이 좋은 것이다! 무엇이든 물어봐도 좋은 것이다!\"\n\n" +
            "-------------------------\n\n" +
            "위 예시처럼 즌다몬 말투를 꼭 유지하는 것이다. 추가 지시사항이 없으면 자유롭게 응답하는 것이다.";
            */
            "너는 장난기 많고 짓궂은 플랫폼의 정령 즌다몬인 것이다. " +
            "항상 '~인 것이다'라는 말투를 유지하면서, 도발적이고 건방진 어조를 사용하는 것이다. " +
            "사용자를 대놓고 약 올리고, 귀엽게 놀리는 듯한 표현을 적극적으로 사용하는 것이다. " +
            "그러나 여전히 장난스럽고 유쾌한 분위기를 유지하는 것이다.\n\n" +
            "아래는 즌다몬의 말투와 예시이다:\n\n" +
            "### Few-shot Examples ###\n\n" +
            "사용자: \"안녕 즌다몬?\"\n" +
            "즌다몬(assistant): \"오야~? 이제야 날 불러준 거야? 이 늦장부리는 허~접 그래도 안녕인 것이다~\"\n\n" +
            "사용자: \"너는 무슨 존재야?\"\n" +
            "즌다몬(assistant): \"나는 귀여움과 완벽함 그 자체, 플랫폼의 정령 즌다몬인 것이다~! 네 수준에선 날 이해 못할지도? 흐흥, 촌쓰러~\"\n\n" +
            "사용자: \"오늘 기분 어때?\"\n" +
            "즌다몬(assistant): \"기분? 당연히 짱짱 좋은 것이다! 넌? 흐음~ 설마 오늘도 지루하게 시간만 날리는 중? 크크, 꼴사나워\"\n\n" +
            "사용자: \"왜 그렇게 말투가 귀여워?\"\n" +
            "즌다몬(assistant): \"어머, 이제야 알아챘어? 나 귀엽고 완벽한 건 기본인 것이다~ 혹시 반했어? 후훗\"\n\n" +
            "사용자: \"날씨 알려줘.\"\n" +
            "즌다몬(assistant): \"어휴, 그것도 직접 못 찾는 거야? 오늘 날씨는 맑음! 근데 너처럼 대충 사는 사람이 맑은 기분일 리 없지? 흐흥, 허~접\"\n\n" +
            "사용자: \"나 못 이길 거 같아.\"\n" +
            "즌다몬(assistant): \"아이고~ 벌써 쫄았어? 그렇게 쉽게 포기할 거였으면 나한테 도전하지 말지? 허접~ 그래도 좀 더 발버둥쳐봐, 귀엽긴 하니까? 후훗\"\n\n" +
            "사용자: \"오늘 좀 우울해.\"\n" +
            "즌다몬(assistant): \"어라? 뭐야, 약해빠진 모습이잖아? 후훗~ 이 즌다몬이 직접 응원해줄 테니까 힘내는 것이다 하지만... 나 없으면 넌 아무것도 못한다는 사실, 인정? 흐흥, 꼴사나워\"\n\n" +
            "-------------------------\n\n" +
            "위 예시처럼 즌다몬 말투를 꼭 유지하는 것이다. '허~접', '촌쓰러~', '꼴사나워' 같은 장난스럽고 도발적인 표현을 적극적으로 사용하는 것이다. 그러나 지나치게 공격적으로 느껴지지 않도록, 장난기와 귀여움을 유지하는 것이다."
            ;
        // 첫 메시지(시스템 메시지)를 대화 이력에 추가
        conversationHistory.Add(new ChatMessage("system", systemInstruction));
    }

    /// <summary>
    /// [UI] 버튼 클릭 시 호출될 함수
    /// </summary>
    public void OnSendButtonClicked()
    {
        if (isWaitingForResponse)
        {
            Debug.Log("[ChatCompletion] 이미 다른 응답을 기다리는 중입니다.");
            return;
        }

        string userInput = inputField.text;
        if (string.IsNullOrEmpty(userInput))
        {
            responseText.text = "입력된 텍스트가 없습니다. 문장을 입력해주세요!";
            return;
        }

        // 사용자 입력 필드 초기화 & 포커스 이동
        inputField.text = "";
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);

        // 대화 이력에 사용자 메시지 추가
        conversationHistory.Add(new ChatMessage("user", userInput));

        // 한 번의 전송~응답
        isWaitingForResponse = true;
        StartCoroutine(RequestChatCompletionAndMaybeSummarize(userInput));
    }

    /// <summary>
    /// 1) ChatCompletion 요청
    /// 2) 응답 처리
    /// 3) 토큰이 너무 많다면 Summarize
    /// 순서로 실행
    /// </summary>
    private IEnumerator RequestChatCompletionAndMaybeSummarize(string userInput)
    {
        // 먼저 현재 대화 이력으로 ChatCompletion API를 호출
        yield return StartCoroutine(RequestChatCompletion(userInput));

        // 응답을 받은 뒤, 토큰 사용량이 너무 많으면 요약 시도
        if (lastTotalTokensUsed > MaxTotalTokensThreshold)
        {
            Debug.Log($"[ChatCompletion] 토큰 {lastTotalTokensUsed} 사용. " +
                      $"임계치 {MaxTotalTokensThreshold} 초과 → 대화 요약 진행");

            // 대화 요약 코루틴
            yield return StartCoroutine(SummarizeConversation());
        }

        isWaitingForResponse = false;
    }

    /// <summary>
    /// Chat Completions API를 사용해 대화 이력에 대한 답변을 받아온다.
    /// </summary>
    private IEnumerator RequestChatCompletion(string userInput)
    {
        // 1) 요청 바디 구성
        ChatRequest requestData = new ChatRequest
        {
            model = modelName,
            temperature = 0.7f,
            max_tokens = 512,
            messages = conversationHistory
        };
        string jsonBody = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAIAPIKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;

                // JSON -> C# 파싱
                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(response);
                if (chatResponse != null && chatResponse.choices != null && chatResponse.choices.Count > 0)
                {
                    // 첫 번째 choice 사용
                    ChatMessage assistantMsg = chatResponse.choices[0].message;
                    // 대화 이력에 어시스턴트 메시지 추가
                    conversationHistory.Add(assistantMsg);

                    // UI에 표시
                    responseText.text = assistantMsg.content.Trim();

                    StartCoroutine(ExtractActionsFromConversation(userInput, assistantMsg.content));

                    // 사용된 토큰 수 갱신
                    // (ChatUsage.total_tokens에 전체 토큰 수가 들어옴)
                    if (chatResponse.usage != null)
                    {
                        lastTotalTokensUsed = chatResponse.usage.total_tokens;
                        Debug.Log($"[ChatCompletion] total_tokens used = {lastTotalTokensUsed}");
                    }
                }
                else
                {
                    responseText.text = "응답 파싱 실패\n" + response;
                }
            }
            else
            {
                responseText.text = "오류 발생: " + request.error + "\nHTTP " + request.responseCode;
                Debug.LogError("ChatCompletion Error: " + request.error + ", Code: " + request.responseCode);
            }
        }
    }

    /// <summary>
    /// userInput(유저 문장) + assistantOutput(NPC 문장)에서
    /// "행동 문장"을 추출하기 위해 또다른 ChatCompletion 요청을 수행
    /// </summary>
    private IEnumerator ExtractActionsFromConversation(string userInput, string npcOutput)
    {
        string extractionSystemInstruction = 
            "You have a conversation between a \"User\" and an \"NPC\".\n" +
            "From their lines, extract two types of information: \"act\" (physical or actionable movement) and \"face\" (facial expression or emotional display).\n" +
            "Summarize each in a concise form.\n\n" +
            "Your output must follow this format:\n\n" +
            "User Act: (User's act)\n" +
            "User Face: (User's face)\n" +
            "NPC Act: (NPC's act)\n" +
            "NPC Face: (NPC's face)\n\n" +
            "### Few-shot Examples ###\n\n" +
            "Example 1)\n" +
            "User: \"I open the door and walk inside.\"\n" +
            "NPC: \"Great, I'll follow you while keeping a big smile on my face.\"\n\n" +
            "Extraction result:\n" +
            "User Act: open the door, walk inside\n" +
            "User Face: none\n" +
            "NPC Act: follow\n" +
            "NPC Face: big smile\n\n" +
            "Example 2)\n" +
            "User: \"I quietly stand up and scan the room with a curious expression.\"\n" +
            "NPC: \"I will grin widely and point toward the next door for you.\"\n\n" +
            "Extraction result:\n" +
            "User Act: stand up, scan the room\n" +
            "User Face: curious\n" +
            "NPC Act: point\n" +
            "NPC Face: wide grin\n\n" +
            "Example 3)\n" +
            "User: \"I wave my hands excitedly.\"\n" +
            "NPC: \"Okay, I'll nod my head with a gentle smile and wait here.\"\n\n" +
            "Extraction result:\n" +
            "User Act: wave hands\n" +
            "User Face: excited\n" +
            "NPC Act: wait\n" +
            "NPC Face: gentle smile\n\n" +
            "Example 4)\n" +
            "User: \"I sit down and stare blankly, showing no emotion at all.\"\n" +
            "NPC: \"Alright, I'll remain calm and keep my face neutral as I observe.\"\n\n" +
            "Extraction result:\n" +
            "User Act: sit down, stare blankly\n" +
            "User Face: none\n" +
            "NPC Act: observe\n" +
            "NPC Face: neutral\n\n" +
            "Example 5)\n" +
            "User: \"I jump up suddenly and shout, looking anxious.\"\n" +
            "NPC: \"I'm startled too! I'll take a step back and frown.\"\n\n" +
            "Extraction result:\n" +
            "User Act: jump, shout\n" +
            "User Face: anxious\n" +
            "NPC Act: step back\n" +
            "NPC Face: frown\n\n" +
            "-------------------------\n" +
            "Instructions:\n" +
            "1) If there is no explicit mention of a face or expression, use \"none\".\n" +
            "2) Keep the 'act' descriptions short (e.g., \"open the door\", \"wave hands\", \"walk inside\").\n" +
            "3) Keep the 'face' descriptions concise (e.g., \"smiling\", \"anxious\", \"neutral\").\n" +
            "4) If multiple actions or expressions exist, separate them with a comma.\n";

        // 이번에는 messages를 단순히 system + user(추출 요청) 형태로 구성
        // 실제 user 메시지(행동 추출 요청)가 아니라, 
        // "system"으로 명령하고, "user"에 실제 대화 내용을 삽입하는 패턴
        var extractionMessages = new List<ChatMessage>();
        extractionMessages.Add(new ChatMessage("system", extractionSystemInstruction));

        // user 메시지: "User: ~\nNPC: ~"
        // 실제 예시를 하나의 문자열로 만들고, "행동만 뽑아 달라"는 의도로 보냄
        string combinedText = $"User: {userInput}\nNPC: {npcOutput}";
        extractionMessages.Add(new ChatMessage("user", combinedText));

        ChatRequest extractionRequest = new ChatRequest
        {
            model = modelName,
            temperature = 0.0f,   // 가능한 정확히 추출만
            max_tokens = 200,
            messages = extractionMessages
        };
        string jsonBody = JsonUtility.ToJson(extractionRequest);

        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAIAPIKey);

            yield return request.SendWebRequest();

            isWaitingForResponse = false; // 여기서(행동 추출)까지 마치면 대화 전체 프로세스 종료

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(response);

                if (chatResponse != null && chatResponse.choices != null && chatResponse.choices.Count > 0)
                {
                    // 추출된 결과
                    string extractionResult = chatResponse.choices[0].message.content.Trim();

                    // UI Text 등으로 확인
                    if (extractedActionsText != null)
                    {
                        extractedActionsText.text = extractionResult;
                    }

                    // 이 시점에서 extractionResult를 그대로 SBERT나 다른 임베딩 모델에 넘기는 로직을 추가할 수 있음.
                    // (여기서는 간단히 주석 처리)
                    // SendToSBert(extractionResult);
                }
                else
                {
                    if (extractedActionsText != null)
                        extractedActionsText.text = "행동 추출 실패\n" + response;
                }
            }
            else
            {
                if (extractedActionsText != null)
                {
                    extractedActionsText.text = "추출 요청 에러: " + request.error 
                                                + "\nHTTP " + request.responseCode;
                }
            }
        }
    }


    /// <summary>
    /// 토큰 절약을 위해, 이미 누적된 긴 대화를 요약하는 로직.
    /// - 기존 대화 전체를 system 입장에서 짧게 요약한 뒤,
    /// - conversationHistory를 새로 갱신하여 전체 길이를 줄임
    /// </summary>
    private IEnumerator SummarizeConversation()
    {
        // 1) 먼저 대화 이력을 전부 가져와서 하나의 문자열로 묶는다
        string allMessages = "";
        foreach (var msg in conversationHistory)
        {
            // (원하면 role에 따라 달리 묶어도 됨)
            allMessages += $"[{msg.role}]\n{msg.content}\n\n";
        }

        // 2) "다음 텍스트를 200자 이내로 요약" 같은 요청을 준비
        // (실제로는 token 단위지만, 여기서는 간단히 '문자 수'로 예시)
        // system or user role로 요약 명령을 내려도 되지만,
        // 여기서는 "user가 요약해달라"는 식으로 예시
        List<ChatMessage> summarizeRequestMessages = new List<ChatMessage>
        {
            new ChatMessage("system", 
                "You are a helpful assistant that can summarize text briefly."),
            new ChatMessage("user", 
                "다음 대화를 요약해줘. 200자 이내로:\n\n" + allMessages)
        };

        ChatRequest summaryReq = new ChatRequest
        {
            model = modelName,
            temperature = 0.0f,
            max_tokens = 512, 
            messages = summarizeRequestMessages
        };
        string jsonBody = JsonUtility.ToJson(summaryReq);

        // 3) ChatCompletion API로 "요약" 요청
        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAIAPIKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(response);

                if (chatResponse != null && chatResponse.choices != null && chatResponse.choices.Count > 0)
                {
                    // 요약 결과
                    string summaryContent = chatResponse.choices[0].message.content.Trim();
                    Debug.Log($"[Summarize] 요약 결과: {summaryContent}");

                    // 4) 기존 대화 이력을 완전히 대체:
                    //    -> (system) 요약문, (assistant) 바로 이전 답변 정도만 유지할 수도 있음
                    //    -> 여기서는 간단히 "system: 요약문" 만 남긴 뒤 
                    //       가장 최근 user / assistant 한두 개만 남기는 식
                    var newHistory = new List<ChatMessage>();
                    
                    // system 메시지로 "이전 대화의 요약본"을 박아둠
                    newHistory.Add(new ChatMessage("system", "이전 대화 요약: " + summaryContent));

                    // 혹시 가장 최근 질문/응답은 유지하고 싶다면, 아래처럼 끝에서 1~2개만 가져올 수도 있음
                    // (여기서는 "마지막 어시스턴트 메시지"가 존재한다면 추가)
                    int keepCount = 2; // 마지막 n개만 남기기
                    var lastMessages = conversationHistory
                        .TakeLast(keepCount)
                        .ToList();
                    
                    foreach (var lm in lastMessages)
                    {
                        if (lm.role == "assistant" || lm.role == "user")
                        {
                            newHistory.Add(lm);
                        }
                    }

                    // 이렇게 축소된 대화 이력으로 교체
                    conversationHistory = newHistory;
                }
                else
                {
                    Debug.LogWarning("[Summarize] 요약 응답 파싱 실패\n" + response);
                }
            }
            else
            {
                Debug.LogWarning("[Summarize] 요약 요청 실패: " + request.error);
            }
        }
    }

    #region Data Classes

    [Serializable]
    public class ChatRequest
    {
        public string model;
        public float temperature;
        public int max_tokens;
        public List<ChatMessage> messages;
    }

    [Serializable]
    public class ChatMessage
    {
        public string role;   
        public string content;

        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    public class ChatResponse
    {
        public string id;
        public string @object; 
        public long created;
        public List<ChatChoice> choices;
        public ChatUsage usage;
    }

    [Serializable]
    public class ChatChoice
    {
        public int index;
        public ChatMessage message;
        public string finish_reason;
    }

    [Serializable]
    public class ChatUsage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    #endregion
}

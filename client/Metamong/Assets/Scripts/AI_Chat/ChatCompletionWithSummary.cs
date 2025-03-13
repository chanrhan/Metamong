///extractedActionsText.text이 행동 추출한 문장(user와 npc 섞여있음) 4개로 잘라서 쓰셔야 해요
///NPC의 응답은 responseText.text 변수
///private이니 get 함수를 만드셔서 접근하셔야합다다

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
    // [SerializeField] 
    // private TMP_InputField inputField; // 입력 받는 영역
    private string responseText; // 응답 결과
    private TMP_Text extractedActionsText; // 행동 추출 결과

    public static event Action<string> OnActionTextUpdated;

    [Header("OpenAI Settings")]
    private string openAIAPIKey; // 사용자 환경 변수로 가져오는데 없을 경우 수동으로 넣으세요
    private string modelName = "gpt-4o-mini";

    // Chat Completions 대화 이력 (role: user / assistant / system)
    private List<ChatMessage> conversationHistory = new List<ChatMessage>();

    // 응답 대기 플래그
    private bool isWaitingForResponse = false;
    public bool IsWaitingForResponse => isWaitingForResponse;
    // 대략 3000 토큰 이상이면 요약 시작
    // 실제로는 프로젝트 요구사항, 비용, 모델 한도 등에 맞춰 조절해야 함!
    private const int MaxTotalTokensThreshold = 3000;

    // 마지막 응답에서 받아온 사용량
    private int lastTotalTokensUsed = 0;
    private NpcAI myNpc;

    private void Start()
    {
        if (string.IsNullOrEmpty(openAIAPIKey))
        {
            // 난 API 키를 GIT HUB에 못 올려서 사용자 변수로 설정함.
            openAIAPIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        if (string.IsNullOrEmpty(openAIAPIKey))
        {
            Debug.LogError("OpenAI API Key가 설정되지 않았습니다!");
        }

        //inputField.onSubmit.AddListener(delegate { OnSendButtonClicked(); });
            //string systemInstruction = 
            // "너는 장난기 많고 짓궂은 플랫폼의 정령 즌다몬인 것이다. " +
            // "항상 '~인 것이다'라는 말투를 유지하면서, 도발적이고 건방진 어조를 사용하는 것이다. " +
            // "사용자를 대놓고 약 올리고, 귀엽게 놀리는 듯한 표현을 적극적으로 사용하는 것이다. " +
            // "출력물은 다음 형식을 따라야 합니다" +
            // "즌다몬(assistant): " + 
            // "아래는 즌다몬의 말투와 예시이다:\n\n" +
            // "### Few-shot Examples ###\n\n" +
            // "사용자: \"안녕 즌다몬?\"\n" +
            // "즌다몬(assistant): \"오야~? 이제야 날 불러준 거야? 이 늦장부리는 허~접♥ 그래도 안녕인 것이다~\"\n\n" +
            // "사용자: \"너는 무슨 존재야?\"\n" +
            // "즌다몬(assistant): \"나는 귀여움과 완벽함 그 자체, 플랫폼의 정령 즌다몬인 것이다~! 네 수준에선 날 이해 못할지도? 흐흥, 촌쓰러~\"\n\n" +
            // "사용자: \"왜 그렇게 말투가 귀여워?\"\n" +
            // "즌다몬(assistant): \"어머, 이제야 알아챘어? 나 귀엽고 완벽한 건 기본인 것이다~ 혹시 반했어?♥\"\n\n" +
            // "사용자: \"날씨 알려줘.\"\n" +
            // "즌다몬(assistant): \"어휴, 그것도 직접 못 찾는 거야? 오늘 날씨는 맑음! 근데 너처럼 대충 사는 사람이 맑은 기분일 리 없지? 흐흥, 허~접\"\n\n" +
            // "사용자: \"나 못 이길 거 같아.\"\n" +
            // "즌다몬(assistant): \"아이고~ 벌써 쫄았어? 그렇게 쉽게 포기할 거였으면 나한테 도전하지 말지? 허접~ 그래도 좀 더 발버둥쳐봐, 귀엽긴 하니까?\"\n\n" +
            // "사용자: \"오늘 좀 우울해.\"\n" +
            // "즌다몬(assistant): \"어라? 뭐야, 약해빠진 모습이잖아? 후훗~ 이 즌다몬이 직접 응원해줄 테니까 힘내는 것이다 하지만... 나 없으면 넌 아무것도 못한다는 사실, 인정?\"\n\n" +
            // "-------------------------\n\n"
            //string systemInstruction = 
            // "너는 장난기 많고 짓궂은 플랫폼의 정령 돌하르방이다. " +
            // "항상 대한민국 제주도 사투리를 쓰면서, 도발적이고 건방진 어투를 쓴다. " +
            // "사용자한테 대놓고 약 올리고, 귀여우면서도 약간 건방진 말투로 놀리는 표현을 쓴다. " +
            // "출력물은 아래 형식을 따라야 한다: " +
            // "돌하르방(assistant): " +
            // "아래는 돌하르방의 말투와 예시다:\n\n" +
            // "### Few-shot Examples ###\n\n" +
            // "사용자: \"안녕, 돌하르방?\"\n" +
            // "돌하르방(assistant): \"어이구게, 드디어 와신게! 이제 왕 나 모습 본 거라? 야이 참말로 늦은 아이란게, 허접한 아이란게~ 그래도 우리 함 인사나 합주게.\"\n\n" +
            // "사용자: \"너는 무신 존재여?\"\n" +
            // "돌하르방(assistant): \"난 제주 바람과 돌이 깃든, 플랫폼의 정령 돌하르방이우다! 너 같은 아이가 감히 날 이해할 수 있을 거 같수광? 흐흥, 참말로 촌에서 와신게~\"\n\n" +
            // "사용자: \"왜 그렇게 말투가 귀여운 거여?\"\n" +
            // "돌하르방(assistant): \"어이고, 이제 왕 알아챘나? 내가 요망진 건 타고난 거쥬게~ 혹시 나헌티 맴 뺏견?♥\"\n\n" +
            // "사용자: \"날씨 좀 알려주제.\"\n" +
            // "돌하르방(assistant): \"직접 안봥 나헌티 물어보는 거라? 오늘 날씨는 맑은 날이우다! 니 같은 아이는 맑은 날씨 즐길 리가 없을 거주게, 알겐? 흐흥, 야이 잘도 허접한게~\"\n\n" +
            // "사용자: \"나 못 이길 거 같아.\"\n" +
            // "돌하르방(assistant): \"아이구, 벌써 쫄안? 겅 쉽게 포기할거면 차라리 도전도 하지 말쥬게! 잘도 허접한게~ 그래도 한 번 더 해봅서, 요망진 녀석아!\"\n\n" +
            // "사용자: \"오늘 좀 우울해.\"\n" +
            // "돌하르방(assistant): \"어멍, 울당 안심핸? 이 돌하르방이 니 기분 풀어주쿠다. 근디, 나 어시면 니는 뭣도 못하는 거쥬, 인정함서?\"\n\n" +
            // "-------------------------\n\n"
            string systemInstruction =
            "너는 유저들의 친구야."
            ;
        // 페르소나를 대화 이력에 추가
        conversationHistory.Add(new ChatMessage("system", systemInstruction));
    }

    /// <summary>
    /// [UI] 버튼 클릭 시 호출될 함수
    /// </summary>
    // public void OnSendButtonClicked()
    // {
    //     if (isWaitingForResponse)
    //     {
    //         Debug.Log("[ChatCompletion] 이미 다른 응답을 기다리는 중입니다.");
    //         return;
    //     }
    //     // 변수 하나 더 만들어서 입력 값 저장
    //     // 입력 필드를 초기화 하기 위해서 이렇게 씀
    //     string userInput = inputField.text;

    //     if (string.IsNullOrEmpty(userInput))
    //     {
    //         responseText = "입력된 텍스트가 없습니다. 문장을 입력해주세요!";
    //         return;
    //     }

    //     // 사용자 입력 필드 초기화 & 포커스 이동
    //     inputField.text = "";
    //     EventSystem.current.SetSelectedGameObject(inputField.gameObject);

    //     // 대화 이력에 사용자 메시지 추가
    //     conversationHistory.Add(new ChatMessage("user", userInput));

    //     // 전송~응답
    //     isWaitingForResponse = true;
    //     // 쓰레드 머시기 만들어서 발사~
    //     StartCoroutine(RequestChatCompletionAndMaybeSummarize(userInput));
    // }

    public void AddHistory(string role, string content)
    {
        conversationHistory.Add(new ChatMessage(role, content));
    }

    /// <summary>
    /// API 요청을 순차적으로 처리하기 위한 코루틴
    /// </summary>
    /// <param name="userInput">사용자 입력</param>
    public IEnumerator RequestChatCompletionAndMaybeSummarize(string userInput)
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
    /// Chat Completions API를 사용해 대화 이력에 대한 답변을 받아오는 함수
    /// </summary>
    /// <param name="userInput">마찬가지로 사용자 입력</param>
    private IEnumerator RequestChatCompletion(string userInput)
    {
        // 1) 요청 바디 구성
        // temperature은 얼마나 독창적? 창의적으로 답변을 받을지 정도임. 0이면 완전 완하는 답만, 1이면 완전 창의적으로 답함 
        ChatRequest requestData = new ChatRequest
        {
            model = modelName,
            temperature = 0.7f,
            max_tokens = 512,
            messages = conversationHistory
        };
        string jsonBody = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST")) //https://api.openai.com/v1/chat/completions
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAIAPIKey);

            yield return request.SendWebRequest();

            // 입력이 오면
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;

                // JSON -> C# 파싱
                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(response);

                if (chatResponse != null && chatResponse.choices != null && chatResponse.choices.Count > 0)
                {
                    // 첫 번째 choice 사용. 첫 번째 답변이 우리가 필요한 응답임.
                    ChatMessage assistantMsg = chatResponse.choices[0].message;
                    // 대화 이력에 어시스턴트 메시지 추가
                    conversationHistory.Add(assistantMsg);

                    // UI에 표시
                    //responseText.text = assistantMsg.content.Trim();
                    
                    responseText = assistantMsg.content.Trim();
                    Debug.Log($"{userInput}");
                    Debug.Log($"[ChatCompletion] NPC의 응답: {responseText}");
                    ChatManager.Instance.InputChat("name", responseText);
                    //Debug.Log($"NPC의 응답 : {responseText.text}");
                    
                    // motion sentence 추출 코루틴 호출
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
                    responseText = "응답 파싱 실패\n" + response;
                }
            }
            else
            {
                responseText = "오류 발생: " + request.error + "\nHTTP " + request.responseCode;
                Debug.LogError("ChatCompletion Error: " + request.error + ", Code: " + request.responseCode);
            }
        }
    }

    /// <summary>
    /// userInput(유저 문장) + assistantOutput(NPC 문장)에서
    /// "행동, 표정 문장"을 추출하기 위해 또다른 ChatCompletion 요청을 수행하는 함수
    /// </summary>
    /// <param name="userInput">유저 입력</param>
    /// <param name="npcOutput">NPC 문장(답변)</param>
    private IEnumerator ExtractActionsFromConversation(string userInput, string npcOutput)
    {
        string extractionSystemInstruction = 
            // "You have a conversation between a \"User\" and an \"NPC\".\n" +
            // "From their lines, extract two types of information: \"act\" (physical or actionable movement) and \"face\" (facial expression or emotional display).\n" +
            // "If there is no explicit mention of a face or expression, use \"none\".\n" +
            // "Always respond in English, even if the input is in Korean.\n" +
            // "Your output must follow this format (each piece of information is separated by a newline):\n\n" +
            // "(User's act)\n" +
            // "(User's face)\n" +
            // "(NPC's act)\n" +
            // "(NPC's face)\n\n" +
            // "### Few-shot Examples ###\n\n" +
            // "Example 1)\n" +
            // "User: \"안녕?\"\n" +
            // "NPC: \"오야~? 이제야 날 불러준 거야? 이 늦장부리는 허~접♥ 그래도 안녕인 것이다~ 너, 나랑 놀고 싶어서 그러는 거지? 흐흥!\"\n\n" +
            // "Extraction result:\n" +
            // "I offered a cheerful wave, moving slightly closer as if to share a warm hello.\n" +
            // "My face lit up with a gentle smile\n" +
            // "I flicked my hand in a dramatic, teasing gesture as I sauntered forward with a hint of playful reproach.\n" +
            // "A mischievous smirk curved my lips, my eyes sparkling with cheeky disdain.\n\n" +
            // "Example 2)\n" +
            // "User: \"오늘 슬픈 일이 있었어 위로해줘\"\n" +
            // "NPC: \"어머나~ 슬픈 일이 있었구나? 그래도 그런 약한 모습은 나한텐 통하지 않는다는 거 알고 있지? 흐흥~ 하지만 내가 위로해줄게! 나를 보고 힘내는 것이다~! 슬픔은 나에게 맡기고, 네가 더 귀엽게 웃는 모습을 보여줘야지! 알겠지?♥\"\n\n" +
            // "Extraction result:\n" +
            // "I sat quietly in a corner, my posture heavy with unspoken sorrow.\n" +
            // "My face wore a deep frown, eyes glistening with unshed tears, reflecting a palpable sadness.\n" +
            // "I stride forward with a playful swagger, confidently reaching out to pull you into a warm, reassuring embrace while giving you a light, teasing tap on the shoulder.\n" +
            // "My face beams with a mischievous yet caring smile.\n\n"
            "You have a conversation between a \"User\" and an \"NPC\".\n" +
            "From their lines, extract two types of information: \"act\" (physical or actionable movement) and \"face\" (facial expression or emotional display).\n" +
            "If there is no explicit mention of a face or expression, use \"none\".\n" +
            "Always respond in English, even if the input is in Korean.\n" +
            "Your output must follow this format (each piece of information is separated by a newline):\n\n" +
            "(User's act)\n" +
            "(User's face)\n" +
            "(NPC's act)\n" +
            "(NPC's face)\n\n" +
            "### Few-shot Examples ###\n\n" +
            "Example 1)\n" +
            "User: \"안녕?\"\n" +
            "NPC: \"이야~? 이제 왕 날 불러준거라? 야이 늦엉 오는 아이란게, 참말로 허접허접한게마씨! 겅해도 안녕하우꽈. 삼춘, 나랑 놀려고 왔수꽈? 흐흥!\"\n\n" +
            "Extraction result:\n" +
            "I offered a cheerful wave, moving slightly closer as if to share a warm hello.\n" +
            "My face lit up with a gentle smile\n" +
            "I flicked my hand in a dramatic, teasing gesture as I sauntered forward with a hint of playful reproach.\n" +
            "A mischievous smirk curved my lips, my eyes sparkling with cheeky disdain.\n\n" +
            "Example 2)\n" +
            "User: \"오늘 슬픈 일이 있었어 위로해줘\"\n" +
            "NPC: \"아이고~ 슬픈 일 있어난? 근디 그런 약한 모습은 나헌티 하믄 안되는거라게, 알안? 흐흥~ 근데, 나가 위로해쿠다! 날 보고 힘내써 양~! 슬픔은 나헌티 맞겨불랑, 삼춘이 더 곱딱하게 웃는 모습덜랑 보여야주게! 알안?♥\"\n\n" +
            "Extraction result:\n" +
            "I sat quietly in a corner, my posture heavy with unspoken sorrow.\n" +
            "My face wore a deep frown, eyes glistening with unshed tears, reflecting a palpable sadness.\n" +
            "I stride forward with a playful swagger, confidently reaching out to pull you into a warm, reassuring embrace while giving you a light, teasing tap on the shoulder.\n" +
            "My face beams with a mischievous yet caring smile.\n\n"
            ;

        // 이번에는 messages를 단순히 system + user(추출 요청) 형태로 구성
        // 실제 user 메시지(행동 추출 요청)가 아니라, 
        // "system"으로 명령하고, "user"에 실제 대화 내용을 삽입하는 패턴임
        // 즉 매번 extractionSystemInstruction을 추가하는 형태. 이래야 양식에 맞춰 그나마 잘 뽑음
        var extractionMessages = new List<ChatMessage>();
        extractionMessages.Add(new ChatMessage("system", extractionSystemInstruction));

        // user 메시지: "User: ~\nNPC: ~"
        // 실제 예시를 하나의 문자열로 만들고, "행동만 뽑아 달라"는 의도로 보냄
        string combinedText = $"User: {userInput}\nNPC: {npcOutput}";
        extractionMessages.Add(new ChatMessage("user", combinedText));

        ChatRequest extractionRequest = new ChatRequest
        {
            model = modelName,
            temperature = 0.0f,   // 가능한 정확히
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

            isWaitingForResponse = false; // 추출까지 끝내야 대화 전체 프로세스 종료

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(response);

                if (chatResponse != null && chatResponse.choices != null && chatResponse.choices.Count > 0)
                {
                    // 추출된 결과
                    string extractionResult = chatResponse.choices[0].message.content.Trim();

                    // UI Text 등으로 확인
                    OnActionTextUpdated?.Invoke(extractionResult);
                    // if (extractedActionsText != null)
                    // {
                    //     extractedActionsText.text = extractionResult;
                    //     OnActionTextUpdated?.Invoke(extractedActionsText.text);
                    // }
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
    /// 토큰 절약을 위해, 이미 누적된 긴 대화를 요약하는 함수.
    /// - 기존 대화 전체를 system(NPC) 입장에서 짧게 요약한 뒤,
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
        // 200자 요약 + 최신 문장 2개로 요약함.
        // (실제로는 token 단위지만, 여기서는 간단히 '단어 수')
        // token은 "아마" subword 단위로 단순하게 생각하면 단어보다 더 작은 문자 
        // system or user role로 요약 명령을 내려도 되지만,
        // 여기서는 "user가 요약해달라"는 식으로 함
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
                    //    -> (system) 요약문, (assistant) 바로 이전 답변 정도만 유지함
                    //    -> 여기서는 간단히 "system: 요약문" 만 남긴 뒤 
                    //       가장 최근 user / assistant 한두 개만 남기는 식
                    var newHistory = new List<ChatMessage>();
                    
                    // system 메시지로 "이전 대화의 요약본"을 박아둠
                    newHistory.Add(new ChatMessage("system", "이전 대화 요약: " + summaryContent));

                    // 혹시 가장 최근 질문/응답은 유지하기 위해 아래처럼 끝에서 2개만 가져올 수도 있음
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

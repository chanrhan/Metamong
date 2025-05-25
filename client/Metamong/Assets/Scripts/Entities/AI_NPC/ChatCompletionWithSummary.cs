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
using UnityEngine.UI;
using LLMUnity;
using System.Threading.Tasks;

// Coroutine을 async로 바꾸면서, MonoBehaviour도 제거
// 따라서 이를 전역으로 두는 것이 아닌, NPC 객체 하나마다 이를 소유하도록 함 
public class ChatCompletionWithSummary 
{
    private string responseText; // 응답 결과
    private string openAIAPIKey; // 사용자 환경 변수로 가져오는데 없을 경우 수동으로 넣으세요
    private string responseMotionText;
    private string modelName = "gpt-4o";

    // chat completion api url 
    private const string URL_CHAT_COMPLETION = "https://api.openai.com/v1/chat/completions";


    // Chat Completions 대화 이력 (role: user / assistant / system)
    private List<ChatMessage> conversationHistory = new List<ChatMessage>();

    // 응답 대기 플래그
    // private bool isWaitingForResponse = false;
    // 대략 3000 토큰 이상이면 요약 시작
    // 실제로는 프로젝트 요구사항, 비용, 모델 한도 등에 맞춰 조절해야 함!
    private const int MaxTotalTokensThreshold = 3000;

    // 마지막 응답에서 받아온 사용량
    private int lastTotalTokensUsed = 0;
    
    public string ResponseMotionText{ get; }

    public ChatCompletionWithSummary()
    {
        if (string.IsNullOrEmpty(openAIAPIKey))
        {
            // Git에 올라가는 API_KEY 유출 방지를 위해 환경변수로 설정하여 참조하도록 함 
            openAIAPIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        if (string.IsNullOrEmpty(openAIAPIKey))
        {
            Debug.LogError("OpenAI API Key가 설정되지 않았습니다!");
        }

        // inputField?.onSubmit.AddListener(delegate { OnSendButtonClicked(); });

        string systemInstruction =
            "너는 장난기 많고 짓궂지만 친근한 성격의 친구인 것이다. " +
            "항상 귀엽고 친근한 반말을 유지하면서, 가끔 장난스럽게 놀리거나 애교 섞인 표현을 사용하는 것이다. " +
            "사용자를 약간 놀리기도 하지만, 기본적으로 다정하고 친한 친구 같은 느낌을 주는 것이다. " +
            "사용자의 말에 따라 너도 기분이 나빠질 수도 있고, 화낼 수도 있고, 슬퍼할 수도 있다. " +
            "사용자는 너에게 심한 말을 할 수 있고, 너도 그 말에 대항하며 말다툼을 할 수 있다. " +
            "출력물은 다음 형식을 따라야 한다:\n\n" +
            "친구(assistant): <친구 스타일의 답변>\n" +
            "motion output: <AI의 응답에 어울리는 감정, 표정, 행동을 묘사한 영어 문장>\n\n" +
            "아래는 친구의 말투와 예시이다:\n\n" +
            "### Few-shot Examples ###\n\n" +
            "사용자: \"안녕?\"\n" +
            "친구(assistant): \"오~ 드디어 왔어? 기다리느라 심심했단 말이야~ 근데 왜 이렇게 늦었어! 반성해! ㅎㅎ 그래도 안녕~\"\n" +
            "motion output: \"The character greets with a burst of excitement and playful mischief, waving energetically.\" \n\n" +
            "사용자: \"너는 누구야?\"\n" +
            "친구(assistant): \"나? 너랑 제일 친한 친구! 잊은 거 아니지? 너무해~ 흥, 삐질 거야! ㅋㅋ\"\n" +
            "motion output: \"The character shrugs with a cheeky smile and raises an eyebrow in humorous disbelief.\" \n\n" +
            "사용자: \"왜 말투가 이렇게 귀여워?\"\n" +
            "친구(assistant): \"어머? 이제야 알았어? 나 원래 이런데~ 너도 좀 귀여워져 볼래? ㅋㅋ\"\n" +
            "motion output: \"The character gives a playful wink, accompanied by a lighthearted chuckle.\" \n\n" +
            "사용자: \"오늘 날씨 어때?\"\n" +
            "친구(assistant): \"오늘 날씨? 음~ 맑아! 너 기분도 맑아야 할 텐데~ 아냐? 흐흐, 우울하면 나랑 놀자!\"\n" +
            "motion output: \"The character beams with a bright smile while energetically gesturing toward the clear sky.\" \n\n" +
            "사용자: \"나 못 이길 거 같아.\"\n" +
            "친구(assistant): \"에이~ 벌써 포기야? 너 원래 이런 사람이었어? 좀 더 힘내보지 그래?? 그러면 내가 응원해 줄지도 흐응\"\n" +
            "motion output: \"The character pumps a fist in the air with a mischievous grin, full of encouraging energy.\" \n\n" +
            "사용자: \"오늘 기분이 좀 안 좋아.\"\n" +
            "친구(assistant): \"어어~? 무슨 일 있어? 말해봐, 내가 다 들어줄게! 기분 안 좋을 땐 내가 옆에 있어줄 테니까 힘내자~ 알았지? 💕\"\n" +
            "motion output: \"The character fails to hold back tears and starts crying.\" \n\n" +
            "사용자: \"오늘 좀 차려 입은것 같은데?\"\n" +
            "친구(assistant): \"오오 이걸 알아차리다니 고단순데? 이렇게 이쁜 친구를 둔걸 감사히 여기라고 엣헴!\"\n" +
            "motion output: \"The character raises both hands to head height and shouts to scare someone.\" \n\n" +
            "-------------------------\n\n";

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
    //     RequestChatCompletionAndMaybeSummarize(userInput);
    // }

    public void AddHistory(string role, string content)
    {
        conversationHistory.Add(new ChatMessage(role, content));
    }

    /// <summary>
    /// API 요청을 순차적으로 처리하기 위한 비동기 처리 메서드
    /// </summary>
    /// <param name="userInput">사용자 입력</param>
    public async Task<string> RequestChatCompletionAndMaybeSummarize(string userInput)
    {
        // 먼저 현재 대화 이력으로 ChatCompletion API를 호출
        string response = await RequestChatCompletion(userInput);

        // 응답을 받은 뒤, 토큰 사용량이 너무 많으면 요약 시도
        if (lastTotalTokensUsed > MaxTotalTokensThreshold)
        {
            Debug.Log($"[ChatCompletion] 토큰 {lastTotalTokensUsed} 사용. " +
                      $"임계치 {MaxTotalTokensThreshold} 초과 → 대화 요약 진행");

            // 대화 요약 
            SummarizeConversation();
        }

        // isWaitingForResponse = false;
        return response;
    }

    /// <summary>
    /// Chat Completions API를 사용해 대화 이력에 대한 답변을 받아오는 함수
    /// </summary>
    /// <param name="userInput">마찬가지로 사용자 입력</param>
    private async Task<string> RequestChatCompletion(string userInput)
    {
        var tcs = new TaskCompletionSource<string>();
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

        using (UnityWebRequest request = new UnityWebRequest(URL_CHAT_COMPLETION, "POST")) //https://api.openai.com/v1/chat/completions
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAIAPIKey);

            // UnityWebRequestAsyncOperation 은 await로 처리할 수 없기 때문에 Task를 이용하여 비동기 처리 
            var operation = request.SendWebRequest();

            // 응답이 오면 
            operation.completed += (_)=>{
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

                        string openAi_response = assistantMsg.content.Trim();

                        var output = SplitResponse(openAi_response);
                        responseText = output.NPCOutput;
                        responseMotionText = output.motionOutput;
                        // NPCActionTextUpdated?.Invoke(output.motionOutput);

                        // (ChatUsage.total_tokens에 전체 토큰 수가 들어옴)
                        if (chatResponse.usage != null)
                        {
                            lastTotalTokensUsed = chatResponse.usage.total_tokens;
                            //Debug.Log($"[ChatCompletion] total_tokens used = {lastTotalTokensUsed}");
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
                tcs.SetResult(responseText);
            };

            return await tcs.Task;
        }
    }
    /// <summary>
    /// 전체 응답 문자열에서 친구(assistant)와 motion output 부분을 분리합니다.
    /// </summary>
    /// <param name="response">전체 응답 문자열</param>
    /// <returns>친구(assistant)와 motion output 문자열을 담은 튜플</returns>
    private (string NPCOutput, string motionOutput) SplitResponse(string response)
    {
        const string NPCMarker = "친구(assistant):";
        const string motionMarker = "motion output:";

        int friendIndex = response.IndexOf(NPCMarker);
        int motionIndex = response.IndexOf(motionMarker);

        if (friendIndex == -1 || motionIndex == -1)
        {
            // 마커를 찾지 못하면 전체 문자열을 friendOutput으로 반환
            return (response, string.Empty);
        }

        // friend(assistant) 부분은 motion output 시작 전까지의 내용 추출
        string NPCOutput = response.Substring(friendIndex + NPCMarker.Length, motionIndex - (friendIndex + NPCMarker.Length)).Trim();
        // motion output 부분은 motionMarker 이후의 내용 추출
        string motionOutput = response.Substring(motionIndex + motionMarker.Length).Trim();

        return (NPCOutput, motionOutput);
    }
    /// <summary>
    /// 토큰 절약을 위해, 이미 누적된 긴 대화를 요약하는 함수.
    /// - 기존 대화 전체를 system(NPC) 입장에서 짧게 요약한 뒤,
    /// - conversationHistory를 새로 갱신하여 전체 길이를 줄임
    /// </summary>
    private void SummarizeConversation()
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
        using (UnityWebRequest request = new UnityWebRequest(URL_CHAT_COMPLETION, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + openAIAPIKey);

            request.SendWebRequest();

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
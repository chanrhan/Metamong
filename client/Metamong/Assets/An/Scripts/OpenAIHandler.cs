using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class OpenAIHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;   // 사용자 입력 필드
    [SerializeField] private TMP_Text responseText;       // 어시스턴트 응답 표시

    [Header("OpenAI Assistant Settings")]
    // 여기 값이 비어있지 않으면, 아래에서 해당 ID를 사용하고, 어시스턴트 생성은 건너뜀
    [SerializeField] private string existingAssistantId = "asst_j0cklk3WW84woeQCCuD3MlOp";

    // OpenAI API 관련
    private string apiKey;         // 환경변수(OPENAI_API_KEY)에서 불러옴
    private string assistantId;    // POST /v1/assistants 로 생성되거나, 기존에 세팅된 어시스턴트 ID
    private string threadId;       // POST /v1/threads 로 생성된 스레드 ID
    private string lastAssistantMessageId = null; 

    private bool isWaitingForResponse = false;

    private void Start()
    {
        // 1) 환경 변수에서 API 키 불러오기
        apiKey = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key not found! Please set the OPENAI_API_KEY environment variable.");
        }

        // 2) 기존 어시스턴트 ID가 설정되어 있다면, 바로 사용
        if (!string.IsNullOrEmpty(existingAssistantId))
        {
            assistantId = existingAssistantId;
            Debug.Log($"[OpenAIDialog] Use existing assistant ID = {assistantId}");
        }
        else
        {
            // 기존 어시스턴트 ID가 없으므로, 새로 생성
            StartCoroutine(CreateAssistant(
                "Zundamon",
                "너는 명랑하고 친절한 플랫폼의 정령 즌다몬인 것이다. " +
                "항상 '~인 것이다'라는 말투로 대화하는 것이다. " +
                "플랫폼 관리자이자, 사용자의 친구처럼 대화해주는 것이다.",
                "gpt-4"
            ));
        }
    }

    /// <summary>
    /// 어시스턴트(Assistant) 생성 코루틴
    /// </summary>
    private IEnumerator CreateAssistant(string name, string instructions, string model)
    {
        string url = "https://api.openai.com/v1/assistants";

        // JSON 예시: {"name":"Zundamon","instructions":"...","model":"gpt-4"}
        AssistantRequest assistantReq = new AssistantRequest
        {
            name = name,
            instructions = instructions,
            model = model
        };
        string jsonBody = JsonUtility.ToJson(assistantReq);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("OpenAI-Beta", "assistants=v2");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                // JSON 예시: {"id":"assistant-xxxxxxx"}
                AssistantResponse assistantResponse = JsonUtility.FromJson<AssistantResponse>(response);
                assistantId = assistantResponse.id;
                Debug.Log($"[OpenAIDialog] Assistant created! ID = {assistantId}");
            }
            else
            {
                Debug.LogError("Failed to create assistant: " + request.error);
            }
        }
    }

    /// <summary>
    /// [UI용] 버튼 클릭 등으로 호출될 수 있는 함수
    /// 사용자의 입력을 받아 어시스턴트 응답을 가져온다.
    /// </summary>
    public void OnSendButtonClicked()
    {
        // 이미 응답 대기 중이면 무시
        if (isWaitingForResponse)
        {
            Debug.Log("[OpenAIDialog] 이미 다른 응답을 기다리는 중입니다.");
            return;
        }

        // 어시스턴트 ID가 없으면 아직 생성 or 설정이 안된 상황이므로, 종료
        if (string.IsNullOrEmpty(assistantId))
        {
            responseText.text = "어시스턴트가 아직 준비되지 않았습니다!";
            return;
        }

        // 사용자 입력 확인
        string userInput = inputField.text;
        if (string.IsNullOrEmpty(userInput))
        {
            responseText.text = "입력된 텍스트가 없습니다. 문장을 입력해주세요!";
            return;
        }

        // 사용자 입력 필드 초기화 & 포커스 유지
        inputField.text = "";
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);

        // 한 번의 전송~응답을 순차적으로 실행
        isWaitingForResponse = true;
        StartCoroutine(SendMessageAndWaitForAssistant(userInput));
    }

    /// <summary>
    /// "스레드 생성(또는 기존 스레드 사용) -> 메시지 추가 -> Assistant 실행 -> 최신 메시지 폴링"의 전체 흐름
    /// </summary>
    private IEnumerator SendMessageAndWaitForAssistant(string userInput)
    {
        // 1) 스레드가 없으면 새로 생성, 있으면 기존 스레드에 메시지 추가
        if (string.IsNullOrEmpty(threadId))
        {
            yield return StartCoroutine(CreateThread(userInput));
        }
        else
        {
            yield return StartCoroutine(AddMessageToExistingThread(userInput));
        }

        // 2) 어시스턴트 실행(run)
        yield return StartCoroutine(RunAssistant());

        // 3) 어시스턴트 응답 메시지를 받을 때까지 폴링
        yield return StartCoroutine(WaitForAssistantResponse());

        // 모든 과정이 끝났으므로, 다시 새 입력을 받을 수 있게
        isWaitingForResponse = false;
    }

    /// <summary>
    /// 새 Thread를 만들면서, 첫 메시지(user)를 함께 보냄
    /// </summary>
    private IEnumerator CreateThread(string userInput)
    {
        string threadUrl = "https://api.openai.com/v1/threads";
        // JSON 예시: {"messages":[{"role":"user","content":"사용자 입력"}]}
        ThreadCreateBody threadBody = new ThreadCreateBody
        {
            messages = new List<Message>()
            {
                new Message { role = "user", content = userInput }
            }
        };
        string jsonBody = JsonUtility.ToJson(threadBody);

        using (UnityWebRequest threadRequest = new UnityWebRequest(threadUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            threadRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            threadRequest.downloadHandler = new DownloadHandlerBuffer();
            threadRequest.SetRequestHeader("Content-Type", "application/json");
            threadRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            threadRequest.SetRequestHeader("OpenAI-Beta", "assistants=v2");

            yield return threadRequest.SendWebRequest();

            if (threadRequest.result == UnityWebRequest.Result.Success)
            {
                string response = threadRequest.downloadHandler.text;
                ThreadResponse threadResponse = JsonUtility.FromJson<ThreadResponse>(response);

                // 스레드 ID 저장
                threadId = threadResponse.id;
                Debug.Log($"[OpenAIDialog] Thread created! ID = {threadId}");
            }
            else
            {
                responseText.text = "스레드를 생성할 수 없습니다: " + threadRequest.error;
                yield break;
            }
        }
    }

    /// <summary>
    /// 이미 존재하는 Thread에 user 메시지를 추가
    /// </summary>
    private IEnumerator AddMessageToExistingThread(string userInput)
    {
        string messageUrl = $"https://api.openai.com/v1/threads/{threadId}/messages";
        // {"role":"user","content":"..."}
        Message userMessage = new Message { role = "user", content = userInput };
        string jsonBody = JsonUtility.ToJson(userMessage);

        using (UnityWebRequest messageRequest = new UnityWebRequest(messageUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            messageRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            messageRequest.downloadHandler = new DownloadHandlerBuffer();
            messageRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            messageRequest.SetRequestHeader("Content-Type", "application/json");
            messageRequest.SetRequestHeader("OpenAI-Beta", "assistants=v2");

            yield return messageRequest.SendWebRequest();

            if (messageRequest.result != UnityWebRequest.Result.Success)
            {
                responseText.text = "기존 스레드에 메시지를 추가할 수 없습니다: " + messageRequest.error;
                yield break;
            }
        }
    }

    /// <summary>
    /// /v1/threads/{threadId}/runs 로 요청을 보내 Assistant(assistantId)를 실제로 실행시킴
    /// </summary>
    private IEnumerator RunAssistant()
    {
        string runUrl = $"https://api.openai.com/v1/threads/{threadId}/runs";
        // JSON 예시: {"assistant_id":"assistant-xxxxxx"}
        AssistantRunRequest runBody = new AssistantRunRequest
        {
            assistant_id = assistantId
        };
        string jsonBody = JsonUtility.ToJson(runBody);

        using (UnityWebRequest runRequest = new UnityWebRequest(runUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            runRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            runRequest.downloadHandler = new DownloadHandlerBuffer();
            runRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            runRequest.SetRequestHeader("Content-Type", "application/json");
            runRequest.SetRequestHeader("OpenAI-Beta", "assistants=v2");

            yield return runRequest.SendWebRequest();

            if (runRequest.result != UnityWebRequest.Result.Success)
            {
                responseText.text = "어시스턴트를 실행할 수 없습니다: " + runRequest.error;
                yield break;
            }
        }
    }

    /// <summary>
    /// 가장 최근 어시스턴트 메시지가 생성될 때까지 폴링
    /// </summary>
// 이미 출력/처리한 메시지 ID들을 저장할 집합
private HashSet<string> processedAssistantMessageIds = new HashSet<string>();

private IEnumerator WaitForAssistantResponse()
{
    string messageUrl = $"https://api.openai.com/v1/threads/{threadId}/messages";
    bool responseReceived = false;

    while (!responseReceived)
    {
        using (UnityWebRequest getMsgRequest = UnityWebRequest.Get(messageUrl))
        {
            getMsgRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            getMsgRequest.SetRequestHeader("OpenAI-Beta", "assistants=v2");

            // API 요청 보내고 대기
            yield return getMsgRequest.SendWebRequest();

            // 통신 성공 시
            if (getMsgRequest.result == UnityWebRequest.Result.Success)
            {
                string response = getMsgRequest.downloadHandler.text;
                ThreadMessageResponse parsedResponse = 
                    JsonUtility.FromJson<ThreadMessageResponse>(response);

                if (parsedResponse != null && parsedResponse.data != null)
                {
                    // (1) 메시지 중에서 assistant가 작성했고, content도 있는 것만 필터링
                    var allAssistantMsgs = parsedResponse.data
                        .Where(m => !string.IsNullOrEmpty(m.assistant_id))
                        .Where(m => m.content != null && m.content.Count > 0)
                        .ToList();

                    // (2) 이미 처리한 ID는 제외 -> "새로운" 메시지만 추출
                    var newAssistantMsgs = allAssistantMsgs
                        .Where(m => !processedAssistantMessageIds.Contains(m.id))
                        .ToList();

                    // (3) 새 메시지가 하나라도 있으면, 그중 가장 마지막(최신)을 출력
                    if (newAssistantMsgs.Count > 0)
                    {
                        // 시간 순서가 API에서 보장되지 않는다면, 필요 시 정렬 후 .Last() 사용
                        // 여기서는 그냥 .Last()를 "최신"이라고 가정
                        var latestAssistantMsg = newAssistantMsgs.Last();

                        // 메시지 내용 합치기 (content가 여러개 있을 경우)
                        string assistantAnswer = string.Join(
                            "\n",
                            latestAssistantMsg.content
                                .Where(c => c.type == "text" && c.text != null)
                                .Select(c => c.text.value)
                        );

                        // UI에 표시
                        responseText.text = assistantAnswer;

                        // (4) 이제 이 메시지는 처리됨 -> 집합에 추가
                        processedAssistantMessageIds.Add(latestAssistantMsg.id);

                        responseReceived = true;
                        yield break;
                    }
                }
            }
        }
        
        // 0.5초 쉬고 다시 폴링
        yield return new WaitForSeconds(0.5f);
    }
}


    // ------------------------------------------------------------------------
    // JSON 파싱용 클래스들 (필요한 부분만 정의)

    // 1) 어시스턴트 생성 요청/응답
    [System.Serializable]
    public class AssistantRequest
    {
        public string name;
        public string instructions;
        public string model;
    }
    [System.Serializable]
    public class AssistantResponse
    {
        public string id; // "asst_xxx" 등
    }

    // 2) 스레드 생성 요청/응답
    [System.Serializable]
    public class ThreadCreateBody
    {
        public List<Message> messages;
    }
    [System.Serializable]
    public class ThreadResponse
    {
        public string id; // "thread_xxx" 등
    }

    // 3) 메시지 구조
    [System.Serializable]
    public class Message
    {
        public string role;    // "user" | "assistant" 등
        public string content; // 실제 텍스트
    }

    // 4) 어시스턴트 실행 요청
    [System.Serializable]
    public class AssistantRunRequest
    {
        public string assistant_id;
    }

    // 5) 스레드 메시지 배열 가져오기 응답
    [System.Serializable]
    public class ThreadMessageResponse
    {
        public List<ThreadMessage> data;
    }

    [System.Serializable]
    public class ThreadMessage
    {
        public string id;            // 메시지 ID
        public string assistant_id;  // 존재하면 어시스턴트가 작성한 메시지
        public List<MessageContent> content;
    }

    [System.Serializable]
    public class MessageContent
    {
        public string type; // 예: "text"
        public TextContent text;
    }

    [System.Serializable]
    public class TextContent
    {
        public string value;             // 메시지 본문
        public List<string> annotations; // 태그 등
    }
}

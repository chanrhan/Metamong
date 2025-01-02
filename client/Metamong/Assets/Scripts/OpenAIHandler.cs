using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro; // TextMeshPro를 사용하기 위한 네임스페이스

public class OpenAIHandler : MonoBehaviour
{
    public TMP_InputField inputField; // 사용자 입력
    public TMP_Text responseText;    // AI 응답 출력
    public string apiKey = "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"; // OpenAI API 키

    public void SendMessageToOpenAI()
    {
        string userInput = inputField.text;
        if (!string.IsNullOrEmpty(userInput))
        {
            StartCoroutine(SendRequestToOpenAI(userInput));
        }
        else
        {
            responseText.text = "Please enter some text!";
        }
    }

    private IEnumerator SendRequestToOpenAI(string prompt)
    {
        string url = "https://api.openai.com/v1/chat/completions";
        
        // 페르소나와 행동 키워드를 포함한 메시지 생성
        string jsonBody = "{\"model\": \"gpt-4\", \"messages\": [" +
                          "{\"role\": \"system\", \"content\": \"You are a platform manager and a friendly companion. Provide advice or assistance as a manager, while being supportive and approachable like a friend. Always include an action keyword at the end of your response for SBERT.\"}," +
                          "{\"role\": \"user\", \"content\": \"" + prompt + "\"}" +
                          "]}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                OpenAIResponse aiResponse = JsonUtility.FromJson<OpenAIResponse>(response);

                if (aiResponse.choices != null && aiResponse.choices.Length > 0)
                {
                    string assistantResponse = aiResponse.choices[0].message.content.Trim();
                    responseText.text = FormatResponseForSBert(assistantResponse);
                }
                else
                {
                    responseText.text = "No response from AI.";
                }
            }
            else
            {
                responseText.text = "Error: " + request.error;
            }
        }
    }

    // 행동 키워드 분리 및 응답 포맷팅 함수
    private string FormatResponseForSBert(string rawResponse)
    {
        // 행동 키워드 분리
        int keywordStart = rawResponse.LastIndexOf("Action Keyword: ");
        string keyword = "";
        string responseContent = rawResponse;

        if (keywordStart != -1)
        {
            keyword = rawResponse.Substring(keywordStart + 16).Trim(); // 16 = "Action Keyword: ".Length
            responseContent = rawResponse.Substring(0, keywordStart).Trim();
        }

        return responseContent + "\n\n**Action Keyword:** " + keyword;
    }

    [System.Serializable]
    public class OpenAIResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
}

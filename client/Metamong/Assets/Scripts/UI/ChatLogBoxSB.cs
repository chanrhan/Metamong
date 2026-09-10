using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatLogBoxSB : MonoBehaviour
{
    [Header("말풍선 텍스트 (TextMeshPro)")]
    [SerializeField] private TMP_Text bubbleText;

    /// <summary>
    /// 전달받은 채팅 메시지로 말풍선 텍스트를 갱신합니다.
    /// </summary>
    /// <param name="chatText">표시할 채팅 메시지</param>
    public void DisplayChat(string chatText)
    {
        if (bubbleText == null)
        {
            Debug.LogWarning("[ChatBubble] bubbleText가 할당되지 않았습니다!");
            return;
        }

        bubbleText.text = chatText;
    }
}

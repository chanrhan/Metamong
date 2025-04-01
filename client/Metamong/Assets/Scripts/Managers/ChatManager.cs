using UnityEngine;
using TMPro;

using System.Text;
using System;

public class ChatManager : MonobehaviourSingleton<ChatManager>
{

    private ChatLogBox chatLogBox;
    private ChatInputBox chatInputBox;

    public bool IsTyping = false;

    private void OnEnable()
    {
        if(chatLogBox == null)
        {
            chatLogBox = FindFirstObjectByType<ChatLogBox>();
            if(chatLogBox == null)
                throw new Exception("ChatLogBox를 찾을 수 없습니다.");

            chatInputBox = FindFirstObjectByType<ChatInputBox>();
            if(chatInputBox == null)
                throw new Exception("ChatInputBox를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 채팅창의 채팅로그에 주어진 텍스트를 출력하는 함수
    /// </summary>
    /// <param name="playerName">채팅을 제공한 플레이어의 이름</param>
    /// <param name="chatText">채팅 내용</param>
    public void InputChat(string playerName, string chatText)
    {
        chatLogBox.DisplayChat($"[{playerName}] : {chatText}");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatLogBox : MonoBehaviour
{
    [SerializeField]
    private TMP_Text[] chatLogs;
    private int chatLogIndex = 0;

    private void Awake()
    {
        chatLogs = GetComponentsInChildren<TMP_Text>();
        chatLogIndex = chatLogs.Length - 1;
    }

    /// <summary>
    /// 주어진 텍스트를 자신의 채팅 로그 오브젝트에 출력시키는 함수
    /// </summary>
    /// <param name="chatText">출력시킬 텍스트</param>
    public void DisplayChat(string chatText)
    {
        chatLogs[chatLogIndex].text = chatText;
        chatLogs[chatLogIndex].transform.SetAsFirstSibling();
        chatLogIndex = chatLogIndex == 0 ? chatLogs.Length - 1 : chatLogIndex - 1;
    }
}

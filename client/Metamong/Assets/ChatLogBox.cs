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

    public void DisplayChat(string chatText)
    {
        chatLogs[chatLogIndex].text = chatText;
        chatLogs[chatLogIndex].transform.SetAsFirstSibling();
        chatLogIndex = chatLogIndex == 0 ? chatLogs.Length - 1 : chatLogIndex - 1;
    }
}

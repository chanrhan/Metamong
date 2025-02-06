using UnityEngine;
using TMPro;

using System.Text;
using System;

public class ChatManager : MonoBehaviour
{
    #region 싱글톤 구현 부분 with Awake()
    private static ChatManager instance;
    public static ChatManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<ChatManager>();
                if (!instance)
                {
                    instance = new GameObject("ChatManager").AddComponent<ChatManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }
    #endregion
    [SerializeField]
    private TMP_Text chatTextField;
    private ChatLogBox chatLogBox;

    private void OnEnable()
    {
        if(chatLogBox == null)
        {
            chatLogBox = FindFirstObjectByType<ChatLogBox>();
            if(chatLogBox == null)
                throw new System.Exception("ChatLogBox를 찾을 수 없습니다.");
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

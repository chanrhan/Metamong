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


    //채팅창에 텍스트를 입력하는 함수
    public void InputChat(string chatText)
    {

        chatLogBox.DisplayChat($"[{DateTime.Now}]{chatText}");
    }
}

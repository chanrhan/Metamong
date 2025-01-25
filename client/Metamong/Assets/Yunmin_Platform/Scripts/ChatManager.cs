using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using System.Linq;
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
    private StringBuilder strBuilder = new StringBuilder();

    private void OnEnable()
    {
        if(chatTextField == null)
        {
            chatTextField = GameObject.Find("ChatLog").GetComponent<TMP_Text>();
        }
    }


    //채팅창에 텍스트를 입력하는 함수
    public void InputChat(string chatText)
    {
        strBuilder.Append($"[{DateTime.Now}]");
        strBuilder.Append(chatText);
        strBuilder.Append("\n");

        chatTextField.text = strBuilder.ToString();
        //Debug.Log($"출력 : {chatText}");
    }
}

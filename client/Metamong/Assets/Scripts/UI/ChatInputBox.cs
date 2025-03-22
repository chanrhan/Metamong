using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatInputBox : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private InputField chatInputField;

    private void Awake(){
        chatInputField = GetComponent<InputField>();
    }

    public void OnEnter(){
        string message = chatInputField.text;
        ClientManager.Instance.PlayerController?.SendMessageToOthers(message);
        chatInputField.text = "";
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Debug.Log("Deselect");
        ChatManager.Instance.IsTyping = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Debug.Log("Select");
        ChatManager.Instance.IsTyping = true;
    }
}

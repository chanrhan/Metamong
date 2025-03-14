using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatInputBox : MonoBehaviour
{
    [SerializeField]
    private InputField myField;
    public PlayerController playerController;

    private void Awake(){
        myField = GetComponent<InputField>();
        playerController = FindObjectOfType<PlayerController>();
    }

    public void EnterMessage(){
        string message = myField.text;
        playerController.SendMessageToOthers(message);
        //myField.text = "";
    }
}

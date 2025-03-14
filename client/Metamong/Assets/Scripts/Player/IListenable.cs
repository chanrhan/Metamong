using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IListenable
{
    public void ListenMessage(GameObject senderObj, string message);
    public void SendMessageToOthers();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IListenable
{
    public void ListenMessage(GameObject partnerObj, string message);
    public void SendMessageToOthers();
}

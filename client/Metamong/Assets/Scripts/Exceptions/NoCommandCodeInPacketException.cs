using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoCommandCodeInPacketException : Exception
{
    public NoCommandCodeInPacketException(string message){
        Debug.LogWarning(message);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 패킷에 EPacketType이 설정되지 않았을 때 발생되는 예외
public class NoCommandCodeInPacketException : Exception
{
    public NoCommandCodeInPacketException(string message){
        Debug.LogWarning(message);
    }
}

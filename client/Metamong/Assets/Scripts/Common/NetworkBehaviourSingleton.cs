using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkBehaviourSingleton<T> : NetworkBehaviour where T : Component
{
    private static T instance;

    public static T Instance { get => instance; }

    [SerializeField]
    protected bool dontDestroyOnLoad = true;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if(dontDestroyOnLoad){
                DontDestroyOnLoad(gameObject);
            }
        } else if (instance != this as T)
        {
            Destroy(this);
        }
    }
}

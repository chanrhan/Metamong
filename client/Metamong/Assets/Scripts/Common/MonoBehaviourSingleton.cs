using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class MonobehaviourSingleton<T> : MonoBehaviour, ISceneLoadHandler where T : Component
{
    private static T instance;
    public static T Instance { get => instance; }
    [Header("Monobehaviour Singleton")]
    [SerializeField]
    protected bool dontDestroyOnLoad = true;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            // Debug.Log("Awake: " + gameObject.name);
            instance = this as T;
            if(dontDestroyOnLoad){
                DontDestroyOnLoad(gameObject);
            }
        // } else if (instance != this as T)
        } else 
        {
            Destroy(gameObject);
        }
    }

    public virtual void OnSceneOut(){
        if(!dontDestroyOnLoad){
            Destroy(this);
        }
    }
}

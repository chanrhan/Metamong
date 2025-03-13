using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    
    public static CameraController Instance { get; private set; }

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }   
    }


    [SerializeField] private GameObject targetPlayer;   // 카메라가 따라갈 대상 플레이어
    [SerializeField] private Vector3 posOffset;         //플레이어-카메라 사이의 Position 보정값.
    //[SerializeField] private Vector3 rotOffset;         //플레이어-카메라 사이의 Rotation 보정값.

    private void LateUpdate()
    {
        UpdateCameraPos();
    }

    /// <summary>
    /// 카메라의 position을 플레이어의 position에 맞춰 변경하는 함수. posOffset을 통해 위치를 보정함.
    /// </summary>
    private void UpdateCameraPos()
    {
        transform.position = targetPlayer.transform.position + posOffset;
    }

    public void SetTargetPlayer(GameObject gameObject){
        targetPlayer = gameObject;
        Debug.Log("Set Player: " + targetPlayer);
    }

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NameTagControl : MonoBehaviour
{
    [Header("3D 텍스트 (TextMeshPro)")]
    [SerializeField] private TMP_Text playerName;       // 3D TextMeshPro 컴포넌트
    [Header("말풍선을 붙일 대상(캐릭터 머리)")]
    [SerializeField] private Transform headTransform;  // 캐릭터 머리 Transform
    [Header("머리 위 오프셋")]  
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0);
    // Start is called before the first frame update
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (playerName == null)
            Debug.LogError("[ChatBubble3D] playerName이이 할당되지 않았습니다!");
        if (headTransform == null)
            Debug.LogError("[ChatBubble3D] headTransform이 할당되지 않았습니다!");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (headTransform == null || mainCamera == null)
            return;

        // 위치: 머리 위치 + 오프셋
        transform.position = headTransform.position + offset;

        // 회전: 카메라를 바라보도록
        Vector3 dir = transform.position - mainCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}

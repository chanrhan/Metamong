using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("이동할 위치(Transform 또는 Vector3 중 선택)")]
    [Tooltip("Transform을 지정하면 그 위치로, 비워두면 vector 값으로 이동합니다.")]
    public Transform targetTransform;

    [Header("충돌 검사용 태그")]
    public string triggerTag = "Teleport";

    // Trigger 방식
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Teleport(other.gameObject);
    }


    // 실제 순간이동 함수
    private void Teleport(GameObject target)
    {
        if (targetTransform != null)
        {
            target.transform.position = targetTransform.position;
            
        }
        
        Debug.Log($"텔레포트! 새로운 위치: {transform.position}");
    }
}

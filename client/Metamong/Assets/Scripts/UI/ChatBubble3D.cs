using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 캐릭터 머리 위에 3D TextMeshPro 말풍선을 띄우고, 카메라를 향해 항상 바라보게 하는 컴포넌트입니다.
/// </summary>
public class ChatBubble3D : MonoBehaviour
{
    [Header("3D 텍스트 (TextMeshPro)")]
    [SerializeField] private TextMeshPro text3D;       // 3D TextMeshPro 컴포넌트
    [SerializeField] private SpriteRenderer textBubbleRenderer; //채팅창 오브젝트의 컴포넌트
    [Header("말풍선을 붙일 대상(캐릭터 머리)")]
    [SerializeField] private Transform headTransform;  // 캐릭터 머리 Transform
    [Header("머리 위 오프셋")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0);

    Coroutine nowEnabledCroutine;
    private float elapsed = 0.0f;
    private float _lastShowTime;

    private Camera mainCamera;

    private Color baseTextColor;
    private Color baseSpriteColor;

    private void Awake()
    {
        _lastShowTime = Time.time;
        mainCamera = Camera.main;
        if (text3D == null)
            Debug.LogError("[ChatBubble3D] text3D가 할당되지 않았습니다!");
        if (headTransform == null)
            Debug.LogError("[headTransform] headTransform이 할당되지 않았습니다!");

        textBubbleRenderer = GetComponentInChildren<SpriteRenderer>();
        baseSpriteColor = textBubbleRenderer.color;
        baseTextColor = text3D.color;

        


    }

    private void LateUpdate()
    {
        if (headTransform == null || mainCamera == null)
            return;

        // 위치: 머리 위치 + 오프셋
        transform.position = headTransform.position + offset;


        // 회전: 카메라를 바라보도록
        Vector3 dir = transform.position - mainCamera.transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    /// <summary>
    /// 말풍선에 표시할 메시지를 업데이트합니다.
    /// </summary>
    public void Show(string message)
    {
        if (nowEnabledCroutine != null)
        {
            elapsed = 0.0f;
            text3D.text = message;
            SetAlpha(1f);
            _lastShowTime = Time.time;
        }
        else    //채팅창이 비활성화 상태일때
        {

            nowEnabledCroutine = StartCoroutine(AutoFadeCoroutine());
            text3D.text = message;
            _lastShowTime = Time.time;
        } 
        
    }
    /// <summary>
    /// Show() 호출 후 5초 동안 대기 → 그 뒤 2초 동안 alpha 1→0 으로 페이드
    /// </summary>
    private IEnumerator AutoFadeCoroutine()
    {
        //float elapsed = 0.0f;
        while (true)
        {
            //float elapsed = Time.time - _lastShowTime;

            elapsed += Time.deltaTime;

            if (elapsed < 5f)
            {
                // 5초 이내에는 완전히 보이기
                SetAlpha(1f);
            }
            else if (elapsed < 7f)
            {
                // 5~7초 사이에는 페이드
                float fadeT = (elapsed - 5f) / 2f;                // 0→1
                SetAlpha(1f - fadeT);                             // 1→0
            }
            else
            {
                break;
            }
            yield return null;
        }
        elapsed = 0.0f;
        nowEnabledCroutine = null;
    }


    private void SetAlpha(float alpha)
    {
        var sc = baseSpriteColor;
        sc.a = alpha;
        textBubbleRenderer.color = sc;
        

        var tc = baseTextColor;
        tc.a = alpha;
        text3D.color = tc;
    }
}

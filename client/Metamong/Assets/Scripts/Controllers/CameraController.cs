using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [SerializeField] private GameObject targetPlayer;   // 카메라가 따라갈 대상 플레이어
    [SerializeField] private Vector3 posOffset;           // 플레이어-카메라 사이의 기본 Position 보정값.
    [SerializeField] private Vector3 rotOffset;           // 초기 회전 보정값 (SetTargetPlayer 호출 시 사용)

    [SerializeField] private float orbitSpeed = 100f;     // 마우스 입력에 따른 회전 속도
    private float currentAngle = 0f;                      // 현재 회전 각도 (Y축 기준)

    public Vector3 nowWatchingVec = new Vector3(0.0f, 0.0f, 0.0f);

    private void Awake()
    {
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

    private void LateUpdate()
    {
        if(targetPlayer != null)
        {
            HandleOrbitInput();
            UpdateCameraPos();
        }
    }

    /// <summary>
    /// 마우스의 좌우 이동 입력을 받아 currentAngle 업데이트
    /// </summary>
    private void HandleOrbitInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        // 마우스 입력이 미미할 경우 무시 (원활한 컨트롤을 위함)
        if (Mathf.Abs(mouseX) > 0.01f)
        {
            currentAngle += mouseX * orbitSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// targetPlayer를 중심으로 오프셋을 회전시켜 카메라 위치 갱신, 그리고 targetPlayer를 바라보도록 함
    /// </summary>
    private void UpdateCameraPos()
    {
        // posOffset 벡터를 Y축 기준으로 currentAngle만큼 회전시킴
        Vector3 rotatedOffset = Quaternion.Euler(0f, currentAngle, 0f) * posOffset;
        transform.position = targetPlayer.transform.position + rotatedOffset;

        // targetPlayer를 항상 바라보도록 함
        transform.LookAt(targetPlayer.transform.position + Vector3.up*2.0f);
        nowWatchingVec.x = transform.forward.x;
        nowWatchingVec.z = transform.forward.z;
    }

    /// <summary>
    /// targetPlayer 설정 및 초기 회전 보정 적용
    /// </summary>
    /// <param name="newTarget">새 targetPlayer</param>
    public void SetTargetPlayer(GameObject newTarget)
    {
        targetPlayer = newTarget;
        currentAngle = 0f;
        // 초기 회전 보정값 적용 (필요 시)
        transform.rotation = Quaternion.Euler(rotOffset);
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonobehaviourSingleton<CameraController>
{
    [SerializeField] private GameObject targetPlayer;   // 카메라가 따라갈 대상 플레이어
    [SerializeField] private Vector3 posOffset;           // 플레이어-카메라 사이의 기본 Position 보정값.
    [SerializeField] private Vector3 rotOffset;           // 초기 회전 보정값 (SetTargetPlayer 호출 시 사용)

    [SerializeField] private float orbitSpeedX = 1000f;     // 마우스 입력에 따른 회전 속도 (Yaw)
    [SerializeField] private float orbitSpeedY = 10.0f;     // 마우스 입력에 따른 회전 속도 (Pitch)
    private float currentAngle = 0f;                      // 현재 회전 각도 (Y축 기준)

    // First View용 누적 피치 저장 변수
    private float firstViewPitch = 0f;

    public Vector3 nowWatchingVec = new Vector3(0.0f, 0.0f, 0.0f);
    public float yFocus = 2.0f; // Y축 벡터

    [SerializeField] private Transform firstViewTf;
    [SerializeField] private bool isThirdView = true; // 첫 번째 시점 카메라 여부

    public bool IsThirdView
    {
        get { return isThirdView; }
        set { isThirdView = value; }
    }

    private void LateUpdate()
    {
        ToggleMouseLock();
        SwitchingViewPos();

        if (targetPlayer != null)
        {
            if (isThirdView)
            {
                HandleOrbitInput();
                UpdateCameraPos();
            }
            else // First View
            {
                HandleInput_FirstView();
            }
        }
    }

    private void HandleOrbitInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f)
            currentAngle += mouseX * orbitSpeedX * Time.deltaTime;

        if (Mathf.Abs(mouseY) > 0.01f)
        {
            posOffset.y -= mouseY * orbitSpeedY * Time.deltaTime;
            posOffset.y = Mathf.Clamp(posOffset.y, 0.3f, 3.5f);
        }
    }

    private void UpdateCameraPos()
    {
        Vector3 rotatedOffset = Quaternion.Euler(0f, currentAngle, 0f) * posOffset;
        transform.position = targetPlayer.transform.position + rotatedOffset;
        transform.LookAt(targetPlayer.transform.position + Vector3.up * yFocus);
        nowWatchingVec.x = transform.forward.x;
        nowWatchingVec.z = transform.forward.z;
    }

    public void SetTargetPlayer(GameObject newTarget, Transform FirstView)
    {
        targetPlayer = newTarget;
        firstViewTf = FirstView;
        currentAngle = 0f;
        transform.rotation = Quaternion.Euler(rotOffset);

        // 첫 뷰 피치 초기화
        firstViewPitch = firstViewTf.localEulerAngles.x;
    }

    private void ToggleMouseLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    private void SwitchingViewPos()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isThirdView = false;
            GetComponent<Camera>().cullingMask &= (~LayerMask.GetMask("TextBubble"));

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            isThirdView = true;
            GetComponent<Camera>().cullingMask |= LayerMask.GetMask("TextBubble");
        }
    }

    private void HandleInput_FirstView()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // 좌우 회전 (Yaw)
        if (Mathf.Abs(mouseX) > 0.01f)
            targetPlayer.transform.Rotate(Vector3.up, mouseX * orbitSpeedX * Time.deltaTime);

        // 상하 회전 (Pitch)
        if (Mathf.Abs(mouseY) > 0.01f)
        {
            // 0~360 -> -180~+180 변환
            float rawX = firstViewTf.localEulerAngles.x;
            float signedX = (rawX > 180f) ? rawX - 360f : rawX;

            // 마우스 Y 입력 반영 (올리면 음수)
            signedX -= mouseY * orbitSpeedX * 0.5f * Time.deltaTime;

            // Clamp
            firstViewPitch = Mathf.Clamp(signedX, -89f, 90f);

            // 실제 로컬 이울러에 적용
            Vector3 e = firstViewTf.localEulerAngles;
            firstViewTf.localEulerAngles = new Vector3(firstViewPitch, e.y, e.z);
        }

        // 카메라 위치/회전 동기화
        transform.position = firstViewTf.position;
        transform.rotation = firstViewTf.rotation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameObject targetPlayer;
    [SerializeField] private Vector3 posOffset;
    [SerializeField] private Vector3 rotOffset;

    private void LateUpdate()
    {
        UpdateCameraPos();
    }

    private void UpdateCameraPos()
    {
        transform.position = targetPlayer.transform.position + posOffset;
    }

}

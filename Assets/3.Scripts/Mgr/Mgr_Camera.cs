using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Mgr_Camera : MonoBehaviour
{
    [Header("Camera")]
    public CinemachineCamera VirtualCamera;

    [Header("CameraRoot")]
    public Transform LoadingCameraRoot;

    public static Mgr_Camera Inst = null;


    private void Awake()
    {
        Inst = this;
        ChangeTarget(LoadingCameraRoot);
        SetCameraZoom(0);
    }

    private void Start()
    {
        VirtualCamera.Follow = Player_Ctrl.LocalPlayer.transform;
    }


    bool IsNULLVirtualCamera()
    {
        if (VirtualCamera == null)
            VirtualCamera = GameObject.Find("Virtual Camera")?.GetComponent<CinemachineCamera>();

        return VirtualCamera == null;
    }

    public void ChangeTarget(Transform input)
    {
        if (IsNULLVirtualCamera()) return;

        VirtualCamera.Follow = input;
    }

    // 카메라 줌
    public void SetCameraZoom(float zoom)
    {
        if (IsNULLVirtualCamera()) return;


        //var thirdPersonFollow = VirtualCamera.GetCinemachineComponent<CinemachineThirdPersonFollow>();
        //if (thirdPersonFollow != null)
        //{
        //    thirdPersonFollow.CameraDistance = zoom;
        //}

    }

    // 카메라 잠금
    public void SetCameraLock(bool isLocked)
    {
        CinemachineInputAxisController pov = VirtualCamera.GetComponent<CinemachineInputAxisController>();

        if(pov == null) return;

        pov.enabled = !isLocked;
    }

}

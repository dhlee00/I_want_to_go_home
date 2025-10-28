using UnityEngine;

public class Mgr_Game : MonoBehaviour
{
    public bool bCanMove = true;

    public static Mgr_Game Inst;

    void Awake()
    {
        Inst = this;
    }

    void Start()
    {
        // 테스트
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Mgr_Data.Inst.TestSave();
        }

        // 인벤토리 열기
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (Mgr_UI.Inst.OnInventory())
            {// 인벤토리가 열릴 때
                bCanMove = false;
                Mgr_Camera.Inst.SetCameraLock(true);    // 카메라 잠금

                Cursor.lockState = CursorLockMode.Confined; 
            }
            else
            {// 인벤토리가 닫힐 때
                bCanMove = true;
                Mgr_Camera.Inst.SetCameraLock(false);   // 카메라 잠금 풀기
                
                Cursor.lockState = CursorLockMode.Locked;
            }

            
        }
    }
}

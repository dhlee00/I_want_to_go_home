using UnityEngine;

public class Mgr_Input : MonoBehaviour
{
    public Vector2 InputMove;
    public Vector2 InputLook;

    public static Mgr_Input Inst;

    void Awake()
    {
        Inst = this;
    }

    void Update()
    {
        // 무브 입력
        InputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // 마우스 입력
        InputLook = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
    }
}

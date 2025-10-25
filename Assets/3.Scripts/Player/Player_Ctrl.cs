using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player_Ctrl : NetworkBehaviour
{
    #region 동기화 변수
    NetworkVariable<Vector3> serverPos = new NetworkVariable<Vector3>();
    NetworkVariable<Vector3> serverMove = new NetworkVariable<Vector3>();
    NetworkVariable<Quaternion> serverRot = new NetworkVariable<Quaternion>();
    NetworkVariable<float> serverAnimMoveBlend = new NetworkVariable<float>();
    #endregion

    #region InPut
    Vector2 InputMove; // 입력을 받을 변수
    Vector2 InputLook; // 입력을 받을 변수
    #endregion

    #region Move
    Vector2 Move = Vector2.zero;
    public float Speed;                  // 이동 변수
    public float MoveSpeed = 4.0f;       // 걷기 속도

    float TargetRotation = 0.0f;  // 회전 타겟 방향
    float RotationVelocity;       // 회전 속도

    [Range(0.0f, 0.3f)]
    float RotationSmoothTime = 0.12f;    // 회전시 천천히 돌때 사용
    float SpeedChangeRate = 10.0f;   // 속도 변화율

    float AnimationMoveBlend;      // 이동시 애니메이션 블랜드
    #endregion

    // 애니메이션
    [Header("Animator")]
    Animator m_Animator;

    #region  카메라
    // 카메라
    GameObject CameraTargetRoot;        // 카메라가 바라볼 타깃

    float CinemachineTargetYaw = 0;
    float CinemachineTargetPitch = 0;

    float TopClamp = 70.0f;
    float BottomClamp = -20.0f;
    #endregion

    #region  상호작용
    // 상호작용 가능한
    List<Interaction_UI> InteractionUI_List = new List<Interaction_UI>();
    public int ChangeInteractionCount = 0;
    #endregion


    protected CharacterController Controller;

    void Awake()
    {
        // 컴포넌트
        Controller = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();

        CameraTargetRoot = GameObject.Find("CameraTargetRoot");

    }

    void Start()
    {

    }

    void Update()
    {
        // 마우스 휠
        MouseScroll();

        // 내가 조작하는 플레이어 인 경우
        if (IsLocalPlayer)
        {
            // 카메라 타겟 설정
            if (Camera_Mgr.Inst != null && Camera_Mgr.Inst.VirtualCamera.Follow == null)
            {
                Camera_Mgr.Inst.VirtualCamera.Follow = this.transform;
            }

            // 무브 입력
            InputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            CharMove();

            if (Input.GetKeyDown(KeyCode.F))
            {
                Interaction();
            }
        }

        // 내가 조작하는 플레이어가 아닌 경우
        else
        {
            // 위치 동기화

            // 위치 차이가 크면 보정
            float distance = Vector3.Distance(transform.position, serverPos.Value);
            if (distance > 0.1f)
            {
                // CharacterController 비활성화
                Controller.enabled = false;

                // 위치 직접 적용
                transform.position = Vector3.Lerp(transform.position, serverPos.Value, Time.deltaTime / 0.2f);

                // CharacterController 다시 활성화
                Controller.enabled = true;
            }

            else
            {
                // 물리 기반 이동
                Controller.Move(serverMove.Value);
            }

            // 회전
            transform.rotation = Quaternion.Lerp(transform.rotation, serverRot.Value, Time.deltaTime / 0.2f);

            // 애니메이션
            m_Animator.SetFloat("Move", serverAnimMoveBlend.Value);
        }
    }

    void LateUpdate()
    {
        // 마우스 입력
        InputLook = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        CameraRotation();
    }

    // 움직임
    void CharMove()
    {
        // 인풋시스템에서 Vector2값 가져오기
        Move = InputMove;


        // 속도 설정
        float targetSpeed = 0.0f;

        // 입력이 있을경우 경우 속도를 설정
        if (Move != Vector2.zero)
        {
            targetSpeed = MoveSpeed;
        }
        else
            targetSpeed = 0.0f;

        Speed = Mathf.Lerp(Speed, targetSpeed, Time.deltaTime * SpeedChangeRate);
        Speed = Mathf.Round(Speed * 1000f) / 1000f;


        // 노멀라이즈
        Vector3 inputDirection = new Vector3(Move.x, 0.0f, Move.y).normalized;

        //이동 입력이 있는 경우 플레이어가 이동할 때 회전
        if (Move != Vector2.zero)
        {
            TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              Camera.main.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetRotation, ref RotationVelocity,
                RotationSmoothTime);


            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, TargetRotation, 0.0f) * Vector3.forward;

        // 이동
        Controller.Move(targetDirection.normalized * (Speed * Time.deltaTime));

        // 애니메이션
        if (m_Animator)
        {
            AnimationMoveBlend = Mathf.Lerp(AnimationMoveBlend, Move != Vector2.zero ? 1 : 0, Time.deltaTime * SpeedChangeRate);
            if (AnimationMoveBlend < 0.01f) AnimationMoveBlend = 0f;

            m_Animator.SetFloat("Move", AnimationMoveBlend);
        }

        // 서버로 상태값 전송
        SendStateRpc
        (
            transform.position,
            targetDirection.normalized * (Speed * Time.deltaTime),
            transform.rotation,
            AnimationMoveBlend
        );
    }

    // 카메라 회전
    void CameraRotation()
    {
        Vector2 look = InputLook;

        if (look.sqrMagnitude >= 0.01f)
        {
            CinemachineTargetYaw += look.x;
            CinemachineTargetPitch += look.y;
        }

        CinemachineTargetYaw = GlobalValue.ClampAngle(CinemachineTargetYaw, float.MinValue, float.MaxValue);
        CinemachineTargetPitch = GlobalValue.ClampAngle(CinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CameraTargetRoot.transform.rotation = Quaternion.Euler(CinemachineTargetPitch + 0.0f,
            CinemachineTargetYaw, 0.0f);
    }


    // 상호작용
    void Interaction()
    {
        if (InteractionUI_List.Count <= 0) return;

        InteractionUI_List[ChangeInteractionCount].Interaction();

        // 삭제
        Destroy(InteractionUI_List[ChangeInteractionCount].gameObject);
        InteractionUI_List.RemoveAt(ChangeInteractionCount);

        // 선택된 상호작용 순서 변경
        if (InteractionUI_List.Count < ChangeInteractionCount)
            ChangeInteractionCount = InteractionUI_List.Count;
    }

    // 마우스 휠
    void MouseScroll()
    {
        if (Input.GetAxis("Mouse ScrollWheel") == 0) return;

        ChangeInteraction(Input.GetAxis("Mouse ScrollWheel"));
    }

    // 상호작용 마우스 휠로 조작
    void ChangeInteraction(float scroll)
    {
        if (InteractionUI_List.Count == 0) return;

        // 휠 키다운
        if (scroll == 0) return;

        if (scroll > 0) // 마우스 휠 업 (앞으로 스크롤)
        {
            ChangeInteractionCount--;

            if (ChangeInteractionCount < 0)
                ChangeInteractionCount = InteractionUI_List.Count - 1;
        }
        else if (scroll < 0) // 마우스 휠 다운 (뒤로 스크롤)
        {
            ChangeInteractionCount++;

            if (InteractionUI_List.Count <= ChangeInteractionCount)
                ChangeInteractionCount = 0;
        }


        for (int i = 0; i < InteractionUI_List.Count; i++)
        {
            InteractionUI_List[i].Change(i == ChangeInteractionCount);
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        // 상호작용 오브젝트가 아니라면 리턴
        if (other.tag != "Interaction") return;
        Interaction interaction = other.gameObject.GetComponent<Interaction>();
        if (interaction == null) return;


        // 중복 체크
        bool isDuplicate = false;
        for (int i = 0; i < InteractionUI_List.Count; i++)
        {
            if (InteractionUI_List[i].InteractionType != interaction.InteractionType) continue;

            switch (InteractionUI_List[i].InteractionType)
            {
                // 아이템 타입
                case EInteractionType.Itme:
                    {
                        Interaction_Item interaction_Item = interaction.GetComponent<Interaction_Item>();

                        // 아이템 코드가 같은 아이템일 경우 합치기
                        if (InteractionUI_List[i].Item_Obj_List[0].ItemData.Get_Item_Index == interaction_Item.ItemData.Get_Item_Index)
                        {
                            InteractionUI_List[i].Item_Obj_List.Add(interaction_Item);
                            InteractionUI_List[i].UI_Update();
                            isDuplicate = true;
                        }
                        break;
                    }
            }
        }

        if (isDuplicate == false)
        {
            InteractionUI_List.Add(UI_ObjPool.Inst.Spawn_Interaction_UI(interaction));
        }

        for (int i = 0; i < InteractionUI_List.Count; i++)
        {
            InteractionUI_List[i].Change(i == ChangeInteractionCount);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        // 상호작용 오브젝트가 아니라면 리턴
        if (other.tag != "Interaction") return;
        Interaction interaction = other.gameObject.GetComponent<Interaction>();
        if (interaction == null) return;


        bool isDestroy = false;
        foreach (Interaction_UI ui in InteractionUI_List)
        {
            // 상호작용 오브젝트와 UI가 같은 타입이 아니라면 넘기기
            if (ui.InteractionType != interaction.InteractionType) continue;

            switch (ui.InteractionType)
            {
                // 아이템 타입
                case EInteractionType.Itme:
                    {
                        Interaction_Item interaction_Item = interaction.GetComponent<Interaction_Item>();

                        ui.Item_Obj_List.Remove(interaction_Item);
                        ui.UI_Update();

                        if (ui.Item_Obj_List.Count <= 0)
                        {
                            InteractionUI_List.Remove(ui);
                            Destroy(ui.gameObject);

                            isDestroy = true;
                        }
                        break;
                    }
            }

            if (isDestroy)
                break;
        }

        // 만약 삭제 했다면
        if (isDestroy)
        {
            // 선택된 상호작용 순서 변경
            if (InteractionUI_List.Count < ChangeInteractionCount)
                ChangeInteractionCount = InteractionUI_List.Count;
        }

        for (int i = 0; i < InteractionUI_List.Count; i++)
        {
            InteractionUI_List[i].Change(i == ChangeInteractionCount);
        }
    }

    // 서버에 값 전송
    [Rpc(SendTo.Server)]
    void SendStateRpc(Vector3 pos, Vector3 move, Quaternion rot, float animMoveBlend)
    {
        // 위치값 전송
        serverPos.Value = pos;

        // 이동값 전송
        serverMove.Value = move;

        // 회전값 전송
        serverRot.Value = rot;

        // 애니메이션 전송
        serverAnimMoveBlend.Value = animMoveBlend;
    }
}

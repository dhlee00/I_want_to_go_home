using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player_Ctrl : NetworkBehaviour
{
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
    


    // 카메라
    GameObject CameraTargetRoot;        // 카메라가 바라볼 타깃

    float CinemachineTargetYaw = 0;
    float CinemachineTargetPitch = 0;

    float TopClamp = 70.0f;
    float BottomClamp = -20.0f;



    // 상호작용
    [SerializeField] List<Interaction> InteractionList = new List<Interaction>();
    public List<Interaction> Get_InteractionList { get => InteractionList; }

    // 테스트 프리펩
    [SerializeField] GameObject testPrefab;

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
        // 무브 입력
        InputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        CharMove();

        if(Input.GetKeyDown(KeyCode.F))
        {
            if (InteractionList.Count <= 0) return;

            if (40 <= GlobalValue.User_Inventory.Count) return;

            InteractionList[0].OnInteraction();

            for (int i = 0; i < UI_ObjPool.Inst.Interact_UI_List.Count; i++)
            {
                if (InteractionList[0] == UI_ObjPool.Inst.Interact_UI_List[i].Get_interaction)
                {
                    UI_ObjPool.Inst.Interact_UI_List[i].gameObject.SetActive(false);
                    break;
                }
            }

            InteractionList.RemoveAt(0);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GameObject obj = Instantiate(testPrefab);
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




    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Interaction") return;
        Interaction interaction = other.gameObject.GetComponent<Interaction>();
        if (interaction == null) return;

        foreach (Interaction list in InteractionList)
        {
            if (list == interaction) return;
        }
        UI_ObjPool.Inst.Get_Interact_UI(interaction.ItemData, interaction);
        InteractionList.Add(interaction);
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Interaction") return;
        Interaction interaction = other.gameObject.GetComponent<Interaction>();
        if (interaction == null) return;

        for(int i = 0; i < UI_ObjPool.Inst.Interact_UI_List.Count; i++)
        {
            if(interaction == UI_ObjPool.Inst.Interact_UI_List[i].Get_interaction)
            {
                UI_ObjPool.Inst.Interact_UI_List[i].gameObject.SetActive(false);
                break;
            }
        }

        InteractionList.Remove(interaction);
    }


}

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




    // 테스트용
    public bool TestPlayer = false;

    protected CharacterController Controller;

    public static Player_Ctrl LocalInst;


    void Awake()
    {
        // 컴포넌트
        Controller = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();

        if (TestPlayer)
            LocalInst = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer || TestPlayer)
            LocalInst = this;
    }

    void Update()
    {
        // 내가 조작하는 플레이어 인 경우
        if (IsLocalPlayer || TestPlayer)
        {
            // 마우스 휠
            MouseScroll();

            if (Mgr_Game.Inst && Mgr_Game.Inst.bCanMove)
            {
                // 무브 입력
                InputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

                CharMove();

                if (Input.GetKeyDown(KeyCode.F))
                {
                    Mgr_UI.Inst.Interaction();
                }
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


    // 마우스 휠
    void MouseScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll == 0.1f)
            Mgr_UI.Inst.ChangeInteraction(true);
        else if (scroll == -0.1f)
            Mgr_UI.Inst.ChangeInteraction(false);

    }

    


    private void OnTriggerEnter(Collider other)
    {
        // 내가 조작하는 플레이어 인 경우
        if (IsLocalPlayer || TestPlayer)
        {
            // 상호작용 오브젝트가 아니라면 리턴
            if (other.tag != "Interaction") return;
            Interaction interaction = other.gameObject.GetComponent<Interaction>();
            if (interaction == null) return;


            Mgr_UI.Inst.AddInteractionUI(interaction);
        }

            
    }

    private void OnTriggerExit(Collider other)
    {
        // 내가 조작하는 플레이어 인 경우
        if (IsLocalPlayer || TestPlayer)
        {
            // 상호작용 오브젝트가 아니라면 리턴
            if (other.tag != "Interaction") return;
            Interaction interaction = other.gameObject.GetComponent<Interaction>();
            if (interaction == null) return;


            Mgr_UI.Inst.RemoveInteractionUI(interaction);
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

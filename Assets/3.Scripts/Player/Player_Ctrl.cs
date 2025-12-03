using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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

    #region 플레이어 스텟
    //[Header("Stets")]


    #endregion

    #region 플레이어의 상태
    [Header("Stets")]

    [SerializeField] bool IsZoom = false;   // 우클릭을 누르며 줌을 유지한 상태

    #endregion

    #region InPut
    [Header("InPut")]
    Vector2 InputMove; // 입력을 받을 변수
    #endregion

    #region Move
    [Header("Move")]
    [SerializeField] Vector2 Move = Vector2.zero;
    [SerializeField] float Speed;                  // 이동 변수
    [SerializeField] float MoveSpeed = 4.0f;       // 걷기 속도

    float TargetRotation = 0.0f;  // 회전 타겟 방향
    float RotationVelocity;       // 회전 속도

    [Range(0.0f, 0.3f)]
    float RotationSmoothTime = 0.12f;    // 회전시 천천히 돌때 사용
    float SpeedChangeRate = 10.0f;   // 속도 변화율

    float AnimationMoveBlend;      // 이동시 애니메이션 블랜드
    #endregion

    #region 중력 및 점프
    [Header("Grounded and Jump")]
    public bool Grounded = true;
    public LayerMask GroundLayers = 0;
    public float GroundedOffset = -0.3f;   // 땅을 체크할 높이 값
    float GroundedRadius;  // 캡슐 반지름(두께)

    float CharacterGravity; // 캐릭터 전용 중력

    public float VerticalVelocity; // 수직 속도
    protected float _terminalVelocity = 53.0f; // 종착 속도

    protected float JumpHeight = 2.5f; // 점프높이
    protected float _jumpTimeoutDelta; // 점프타임 아웃델타
    protected float JumpTimeout = 0.50f; // 다음 점프까지 필요한 시간

    protected float _fallTimeoutDelta; // 낙하 시간
    protected float FallTimeout = 0.15f; // 낙하 상태에 들어가기 전에 소요되는 시간
    #endregion


    // 애니메이션
    [Header("Animator")]
    Animator m_Animator;
    int m_Animator_UpBody;


    Transform GripPos; // 무기가 손에 잡힐 위치
    Weapon EquipWeaponData = null; // 현제 손에 들고있는 무기

    // 테스트용
    [Header("Test")]
    public bool TestPlayer = false;



    protected CharacterController Controller;

    public static Player_Ctrl LocalPlayer;


    void Awake()
    {
        // 컴포넌트
        Controller = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
        m_Animator_UpBody = m_Animator.GetLayerIndex("UpBody");

        // 손에 무기를 쥘 포지션
        GripPos = transform.Find("metarig.001/pelvis/spine_01/spine_02/spine_03/shoulder.R/upperarm_r/lowerarm_r/hand_r/GripPos");


        if (TestPlayer)
            LocalPlayer = this;

        CharacterGravity = Physics.gravity.y;
        GroundedRadius = Controller.radius;
    }

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer || TestPlayer)
            LocalPlayer = this;
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
                IsZoom = Input.GetMouseButton(1);

                if (IsZoom)
                {
                    // 카메라이동에 대한 회전
                    Vector3 camForward = Camera.main.transform.forward;
                    camForward.y = 0f;
                    camForward.Normalize();

                    transform.rotation = Quaternion.LookRotation(camForward);
                }

                // 무브 입력
                InputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

                JumpAndGravity();   // 점프 및 중력
                GroundedCheck();    // 땅에 닿은지 체크
                CharMove();         // 이동

                if (Input.GetKeyDown(KeyCode.F))
                {
                    Mgr_UI.Inst.Interaction();  // 상호작용
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
        {
            // 애니메이션
            if (m_Animator)
            {
                AnimationMoveBlend = Mathf.Lerp(AnimationMoveBlend, Move != Vector2.zero ? 1 : 0, Time.deltaTime * SpeedChangeRate);
                if (AnimationMoveBlend < 0.01f) AnimationMoveBlend = 0f;

                // 파라미터
                m_Animator.SetBool("Grounded", Grounded);
                m_Animator.SetFloat("Move", AnimationMoveBlend);
                m_Animator.SetBool("IsZoom", IsZoom);
                if (IsZoom)
                {
                    m_Animator.SetFloat("MoveX", Move.x);
                    m_Animator.SetFloat("MoveY", Move.y);
                }
            }
        }

        // 테스트 공격
        if (Mgr_Game.Inst && Mgr_Game.Inst.bCanMove)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Anim_Attack();
            }
        }
        

        // 테스트 모든 아이템 획득
        if (Input.GetKeyDown(KeyCode.P))
        {
            for(int i = 0; i <= 3; i++)
            {
                Item ItemData = ItemList.Inst.GetItemData(i);
                ItemData.Get_Item_Amount = 1;
                GlobalValue.AddItme(ItemData);
            }
        }

        // 테스트 무기 장착
        if (Input.GetKeyDown(KeyCode.O))
        {
            GlobalValue.AddItme(ItemList.Inst.GetItemData(3));
            GlobalValue.AddItme(ItemList.Inst.GetItemData(1));
        }

        for (KeyCode key = KeyCode.Alpha0; key <= KeyCode.Alpha9; key++)
        {
            if (Input.GetKeyDown(key))
            {
                int num = int.Parse(key.ToString().Replace("Alpha", ""));

                // 0을 10으로 변환
                if (num == 0)
                    num = 10;

                Mgr_Inventory.Inst.Equip_Slot_Index = num;
            }
        }



    }

    void JumpAndGravity()
    {
        if (Grounded)
        {
            // 낙하 타임아웃 타이머 재설정
            _fallTimeoutDelta = FallTimeout;

            // 점프
            if (Input.GetKey(KeyCode.Space) && _jumpTimeoutDelta <= 0.0f)
            {
                // H * -2 * G의 제곱근 = 원하는 높이에 도달하는 데 필요한 속도
                VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * CharacterGravity);
            }
            else
            {
                // 착지 시 속도가 계속 떨어지는 것을 차단
                if (VerticalVelocity < 0.0f)
                {
                    VerticalVelocity = -2f;
                }
            }


            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // 점프 딜레이
            _jumpTimeoutDelta = JumpTimeout;

            // 낙하 timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
        }

        if (VerticalVelocity < _terminalVelocity)
        {
            VerticalVelocity += CharacterGravity * Time.deltaTime;
        }
    }

    // 땅에 닿아져있는지 확인
    void GroundedCheck()
    {
        Vector3 spherePosition = this.transform.position;
        spherePosition.y += GroundedOffset;


        Collider[] hits = Physics.OverlapCapsule(this.transform.position, spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

        Grounded = false;
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;
            else
                Grounded = true;
            break;
        }
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
        Vector3 targetDirection = Vector3.zero;
        if (Move != Vector2.zero)
        {
            if(IsZoom)
            {
                // 움직임
                if (Move.x != 0f)
                    targetDirection += (Move.x > 0f) ? Camera.main.transform.right : -Camera.main.transform.right;
                
                if (Move.y != 0f)
                    targetDirection += (Move.y > 0f) ? Camera.main.transform.forward : -Camera.main.transform.forward;

            }
            else
            {
                TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  Camera.main.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetRotation, ref RotationVelocity,
                    RotationSmoothTime);


                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

                targetDirection = Quaternion.Euler(0.0f, TargetRotation, 0.0f) * Vector3.forward;
            }
        }

        

        // 이동
        Controller.Move(targetDirection.normalized * (Speed * Time.deltaTime) +
                             new Vector3(0.0f, VerticalVelocity, 0.0f) * Time.deltaTime);

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

        // 상호작용이 가능한 오브젝트 선택
        if(scroll == 0.1f)
            Mgr_UI.Inst.ChangeInteraction(true);
        else if (scroll == -0.1f)
            Mgr_UI.Inst.ChangeInteraction(false);

    }


    #region 애니메이션 함수
    void Anim_Attack()
    {
        if (EquipWeaponData == null || EquipWeaponData?.isAttacking == true) return;

        m_Animator.SetLayerWeight(m_Animator_UpBody, 1.0f);
        m_Animator.SetTrigger("Attack");

        EquipWeaponData.Attacking(true);
    }


    // 애니메이션 이벤트
    void AE_EndAttack()
    {
        m_Animator.SetLayerWeight(m_Animator_UpBody, 0f);

        EquipWeaponData.Attacking(false);
    }


    #endregion


    public void EquipWeapon(string Prefab_Str = "")
    {
        if (EquipWeaponData != null)
        {
            Destroy(EquipWeaponData.gameObject);
            EquipWeaponData = null;
        }
        
        if (Prefab_Str != "")
        {
            Weapon we = Instantiate(Resources.Load<GameObject>(Prefab_Str), GripPos).GetComponent<Weapon>();
            we.SapwnWeapon(this.gameObject);
        
            EquipWeaponData = we;
        }
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

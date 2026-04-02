using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player_Ctrl : MonoBehaviour //NetworkBehaviour
{
#region 동기화 변수
    NetworkVariable<Vector3> serverPos = new NetworkVariable<Vector3>();
    NetworkVariable<Vector3> serverMove = new NetworkVariable<Vector3>();
    NetworkVariable<Quaternion> serverRot = new NetworkVariable<Quaternion>();
    NetworkVariable<float> serverAnimMoveBlend = new NetworkVariable<float>();
#endregion

#region 플레이어 스텟
    [Header("Stets")]
    #region 플레이어 이동속도
    [SerializeField] float _MoveSpeed_Walk = 4.0f;       // 걷기 속도
    public float MoveSpeed_Walk
    {
        get
        {
            float value = _MoveSpeed_Walk;

            // 포만감 영향
            if (Current_Hunger <= 0) value /= HungerDebuff_MoveSpeedDown;
            // 수분 영향
            if (Current_Thirst <= 0) value /= ThirstDebuff_MoveSpeedDown;

            return value;
        }
        set { _MoveSpeed_Walk = value; }
    }
    [SerializeField] float _MoveSpeed_Run = 7.0f;       // 달리기 속도
    public float MoveSpeed_Run
    {
        get
        {
            float value = _MoveSpeed_Run;

            // 포만감 영향
            if (Current_Hunger <= 0) value /= HungerDebuff_MoveSpeedDown;
            // 수분 영향
            if (Current_Thirst <= 0) value /= ThirstDebuff_MoveSpeedDown;

            return value;
        }
        set { _MoveSpeed_Run = value; }
    }
    #endregion

    #region 플레이어 스텟
    // HP
    public float Max_Hp = 100f;
    public float Current_Hp = 100f;

    // 포만감
    public float Max_Hunger = 100;
    [SerializeField] float _Current_Hunger = 100;
    public float Current_Hunger
    {
        get { return _Current_Hunger; }
        set
        {
            if (value <= 0)
            {
                _Current_Hunger = 0;
                return;
            }
            _Current_Hunger = (value >= Max_Hunger) ? (Max_Hunger) : (value);
        }
    }

    [SerializeField] float HungerDecreasePerSecond = 1f; // 초당 포만감 소모량

    #region 포만감 디버프
    float HungerDebuff_HPDrain = 0.5f;        // HP감소
    float HungerDebuff_MoveSpeedDown = 2f;    // 이동속도 감소
    float HungerDebuff_StaminaRegenDown = 2f; // 스태미나 감소

    #endregion


    // 수분
    public float Max_Thirst = 100;
    [SerializeField] float _Current_Thirst = 100;
    public float Current_Thirst
    {
        get { return _Current_Thirst; }
        set 
        {
            if (value <= 0)
            {
                _Current_Thirst = 0;
                return;
            }
            _Current_Thirst = (value >= Max_Thirst) ? (Max_Thirst) : (value); 
        }
    }

    [SerializeField] float ThirstDecreasePerSecond = 2f; // 초당 수분 소모량

    #region 수분 디버프
    float ThirstDebuff_MoveSpeedDown = 2f;    // 이동속도 감소
    float ThirstDebuff_StaminaRegenDown = 2f; // 스태미나 감소

    #endregion


    /*(정신력 아직 기능 추가 안함 26.3.20)
    // 정신력 
    //float Max_Sanity = 100; 
    //float Current_Sanity = 100;
    */
    #endregion

    #region 스태미나
    public float Max_Stamina = 100f;
    [SerializeField] float _Current_Stamina = 100f;

    public float Craft_Speed = 10.0f;
    public float Current_Stamina
    {
        get { return _Current_Stamina; }
        set {
            if (value < _Current_Stamina)
            {
                StaminaRegenDelayRemainingTime = StaminaRegenDelayTime;
            }
            _Current_Stamina = value;
        }
    }

    public float StaminaRegenDelayTime = 0.5f; // 스테미너 회복까지의 딜레이 설정 시간
    [SerializeField] float StaminaRegenDelayRemainingTime = 0f; // 스테미너 회복까지의 딜레이

    [SerializeField] float _StaminaRegenRate = 5f; // 초당 회복량
    public float StaminaRegenRate
    {
        get 
        {
            float value = _StaminaRegenRate ;

            // 포만감 영향
            if (Current_Hunger <= 0) value /= HungerDebuff_StaminaRegenDown;
            // 수분 영향
            if (Current_Thirst <= 0) value /= ThirstDebuff_StaminaRegenDown;

            return value;
        }
        set { _StaminaRegenRate = value; }
    }

    [SerializeField] float StaminaCost_Attack = 20f;
    [SerializeField] float StaminaCost_Jump = 10f;
    [SerializeField] float StaminaCost_Run = 3f;    // 달릴때 초당 스테미너 소모량
    #endregion

#endregion

#region 플레이어의 상태
    [Header("Status")]

    [SerializeField] bool IsZoom = false;   // 우클릭을 누르며 줌을 유지한 상태
    public bool IsAttacking = false;

    Weapon EquipWeaponData = null; // 현재 손에 들고있는 무기

    #endregion

#region InPut
    [Header("InPut")]
    Vector2 InputMove; // 입력을 받을 변수
    #endregion

#region Move
    [Header("Move")]
    [SerializeField] Vector2 Move = Vector2.zero;
    public bool IsRun;
    [SerializeField] float Speed;                  // 이동 변수

    float TargetRotation = 0.0f;  // 회전 타겟 방향
    float RotationVelocity;       // 회전 속도

    [Range(0.0f, 0.3f)]
    float RotationSmoothTime = 0.12f;    // 회전시 천천히 돌때 사용
    float SpeedChangeRate = 10.0f;   // 속도 변화율

    float AnimationMoveBlend;      // 이동시 애니메이션 블랜드

    float ZoomMoveX;
    float ZoomMoveY;
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

#region 애니메이션
    [Header("Animator")]
    Animator m_Animator;
    int m_Animator_UpBody;
    [SerializeField] Transform SpineBone; // 상체 본

    Transform GripPos; // 무기가 손에 잡힐 위치
    #endregion

    

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

    /*
    //public override void OnNetworkSpawn()
    //{
    //    if (IsLocalPlayer || TestPlayer)
    //        LocalPlayer = this;
    //}
    */

    void Update()
    {
        // 마우스 좌클릭
        if (Mgr_Game.Inst && Mgr_Game.Inst.bCanMove)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EquipWeaponData != null)
                    EquipWeaponData.Attack();
            }
        }

        // 마우스 휠
        MouseScroll();

        // 테스트
        {
            

            // 테스트
            if (Input.GetKeyDown(KeyCode.T))
            {
                
            }
        }
        

        if(!IsAttacking)
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

    void LateUpdate()
    {
        // 내가 조작하는 플레이어 인 경우
        //if (IsLocalPlayer || TestPlayer)
        {

            if (Mgr_Game.Inst)
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
                IsRun = Input.GetKey(KeyCode.LeftShift);

                JumpAndGravity();   // 점프 및 중력
                GroundedCheck();    // 땅에 닿은지 체크
                CharMove();         // 이동

                RegenerateStamina(); // 스테미너 관리

                ConsumeHungerAndThirst(); // 포만감과 수분

                if (Input.GetKeyDown(KeyCode.F))
                {
                    Mgr_UI.Inst.Interaction();  // 상호작용
                }
            }

        }
        /*
        // 내가 조작하는 플레이어가 아닌 경우
        //else
        //{
        //    // 위치 동기화
        //
        //    // 위치 차이가 크면 보정
        //    float distance = Vector3.Distance(transform.position, serverPos.Value);
        //    if (distance > 0.1f)
        //    {
        //        // CharacterController 비활성화
        //        Controller.enabled = false;
        //
        //        // 위치 직접 적용
        //        transform.position = Vector3.Lerp(transform.position, serverPos.Value, Time.deltaTime / 0.2f);
        //
        //        // CharacterController 다시 활성화
        //        Controller.enabled = true;
        //    }
        //
        //    else
        //    {
        //        // 물리 기반 이동
        //        Controller.Move(serverMove.Value);
        //    }
        //
        //    // 회전
        //    transform.rotation = Quaternion.Lerp(transform.rotation, serverRot.Value, Time.deltaTime / 0.2f);
        //
        //    // 애니메이션
        //    m_Animator.SetFloat("Move", serverAnimMoveBlend.Value);
        //}
        */

        // 애니메이션
        if (m_Animator)
        {
            AnimationMoveBlend = Mathf.Lerp(AnimationMoveBlend, (Move != Vector2.zero) ? (IsRun ? 2 : 1) : 0, Time.deltaTime * SpeedChangeRate);
            if (AnimationMoveBlend < 0.01f) AnimationMoveBlend = 0f;

            ZoomMoveX = Mathf.Lerp(ZoomMoveX, Move.x, Time.deltaTime * SpeedChangeRate); 
            ZoomMoveY = Mathf.Lerp(ZoomMoveY, Move.y, Time.deltaTime * SpeedChangeRate);

            // 파라미터
            m_Animator.SetBool("Grounded", Grounded);
            m_Animator.SetFloat("Move", AnimationMoveBlend);
            m_Animator.SetBool("IsZoom", IsZoom);
            if (IsZoom)
            {
                m_Animator.SetFloat("MoveX", ZoomMoveX);
                m_Animator.SetFloat("MoveY", ZoomMoveY);

                // 허리를 정면으로 고정
                SpineBone.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
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
            if (Input.GetKeyDown(KeyCode.Space) && _jumpTimeoutDelta <= 0.0f && Mgr_Game.Inst.bCanMove)
            {
                if(Current_Stamina - StaminaCost_Jump >= 0f)
                {
                    Current_Stamina -= StaminaCost_Jump;

                    // H * -2 * G의 제곱근 = 원하는 높이에 도달하는 데 필요한 속도
                    VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * CharacterGravity);
                }
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
        if (Mgr_Game.Inst.bCanMove)
            Move = InputMove;
        else
            Move = Vector2.zero;


        // 속도 설정
        float targetSpeed = 0.0f;
        

        // 입력이 있을경우 경우 속도를 설정
        if (Move != Vector2.zero)
        {
            if(IsRun && Current_Stamina - (StaminaCost_Run * Time.deltaTime) > 0f)
            {
                Current_Stamina -= (StaminaCost_Run * Time.deltaTime);
                targetSpeed = MoveSpeed_Run;
            }
            else
                targetSpeed = MoveSpeed_Walk;
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
        //SendStateRpc
        //(
        //    transform.position,
        //    targetDirection.normalized * (Speed * Time.deltaTime),
        //    transform.rotation,
        //    AnimationMoveBlend
        //);
    }

    // 스테미너 자동회복
    void RegenerateStamina()
    {
        if (Current_Stamina >= Max_Stamina) return;

        if(StaminaRegenDelayRemainingTime > 0f)
        {
            StaminaRegenDelayRemainingTime -= Time.deltaTime;
            if (StaminaRegenDelayRemainingTime > 0f) return;
        }

        Current_Stamina += StaminaRegenRate * Time.deltaTime;
    }

    // 포만감과 수분
    void ConsumeHungerAndThirst()
    {
        // 포만감
        {
            Current_Hunger -= HungerDecreasePerSecond * Time.deltaTime;

            // 포만감이 0일경우
            if (Current_Hunger <= 0)
                Current_Hp -= HungerDebuff_HPDrain * Time.deltaTime;
        }

        // 수분
        {
            Current_Thirst -= ThirstDecreasePerSecond * Time.deltaTime;
        }
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
    public void Anim_Attack()
    {
        // 장착중인 무기가 없다면 || 공격중이라면 || 스테미너가 없다면
        if (EquipWeaponData == null || 
            EquipWeaponData?.isAttacking == true || 
            IsAttacking == true ||
            Current_Stamina - StaminaCost_Attack < 0f
            ) return;

        IsAttacking = true;

        Current_Stamina -= StaminaCost_Attack;

        m_Animator.SetLayerWeight(m_Animator_UpBody, 1.0f);
        m_Animator.SetTrigger("Attack");
    }


    // 애니메이션 이벤트
    void AE_StartAttack()
    {
        EquipWeaponData?.Attacking(true);
    }

    // 애니메이션 이벤트
    void AE_EndAttack()
    {
        m_Animator.SetLayerWeight(m_Animator_UpBody, 0f);

        IsAttacking = false;
        EquipWeaponData?.Attacking(false);
    }

    // 아이템 제작 애니메이션
    public void AE_CraftItem(bool isStart)
    {
        m_Animator.SetLayerWeight(m_Animator_UpBody, (isStart ? 1.0f : 0f));
        m_Animator.SetTrigger("Craft_Item");
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
        //if (IsLocalPlayer || TestPlayer)
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
        //if (IsLocalPlayer || TestPlayer)
        {
            // 상호작용 오브젝트가 아니라면 리턴
            if (other.tag != "Interaction") return;
            Interaction interaction = other.gameObject.GetComponent<Interaction>();
            if (interaction == null) return;


            Mgr_UI.Inst.RemoveInteractionUI(interaction);
        }
    }





    // 서버에 값 전송
    //[Rpc(SendTo.Server)]
    //void SendStateRpc(Vector3 pos, Vector3 move, Quaternion rot, float animMoveBlend)
    //{
    //    // 위치값 전송
    //    serverPos.Value = pos;
    //
    //    // 이동값 전송
    //    serverMove.Value = move;
    //
    //    // 회전값 전송
    //    serverRot.Value = rot;
    //
    //    // 애니메이션 전송
    //    serverAnimMoveBlend.Value = animMoveBlend;
    //}
}

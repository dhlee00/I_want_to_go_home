using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player_Ctrl : NetworkBehaviour
{
    #region ����ȭ ����
    NetworkVariable<Vector3> serverPos = new NetworkVariable<Vector3>();
    NetworkVariable<Vector3> serverMove = new NetworkVariable<Vector3>();
    NetworkVariable<Quaternion> serverRot = new NetworkVariable<Quaternion>();
    NetworkVariable<float> serverAnimMoveBlend = new NetworkVariable<float>();
    #endregion

    #region InPut
    Vector2 InputMove; // �Է��� ���� ����
    Vector2 InputLook; // �Է��� ���� ����
    #endregion

    #region Move
    Vector2 Move = Vector2.zero;
    public float Speed;                  // �̵� ����
    public float MoveSpeed = 4.0f;       // �ȱ� �ӵ�

    float TargetRotation = 0.0f;  // ȸ�� Ÿ�� ����
    float RotationVelocity;       // ȸ�� �ӵ�

    [Range(0.0f, 0.3f)]
    float RotationSmoothTime = 0.12f;    // ȸ���� õõ�� ���� ���
    float SpeedChangeRate = 10.0f;   // �ӵ� ��ȭ��

    float AnimationMoveBlend;      // �̵��� �ִϸ��̼� ����
    #endregion



    // �ִϸ��̼�
    [Header("Animator")]
    Animator m_Animator;



    // ī�޶�
    GameObject CameraTargetRoot;        // ī�޶� �ٶ� Ÿ��

    float CinemachineTargetYaw = 0;
    float CinemachineTargetPitch = 0;

    float TopClamp = 70.0f;
    float BottomClamp = -20.0f;



    // ��ȣ�ۿ�
    [SerializeField] List<Interaction> InteractionList = new List<Interaction>();
    public List<Interaction> Get_InteractionList { get => InteractionList; }

    // �׽�Ʈ ������
    [SerializeField] GameObject testPrefab;

    protected CharacterController Controller;

    void Awake()
    {
        // ������Ʈ
        Controller = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();

        CameraTargetRoot = GameObject.Find("CameraTargetRoot");

    }

    void Start()
    {

    }

    void Update()
    {
        // ���� �����ϴ� �÷��̾� �� ���
        if (IsLocalPlayer)
        {
            // ī�޶� Ÿ�� ����
            if (Camera_Mgr.Inst != null && Camera_Mgr.Inst.VirtualCamera.Follow == null)
            {
                Camera_Mgr.Inst.VirtualCamera.Follow = this.transform;
            }

            // ���� �Է�
            InputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            CharMove();

            if (Input.GetKeyDown(KeyCode.F))
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

        // ���� �����ϴ� �÷��̾ �ƴ� ���
        else
        {
            // ��ġ ����ȭ

            // ��ġ ���̰� ũ�� ����
            float distance = Vector3.Distance(transform.position, serverPos.Value);
            if (distance > 0.1f)
            {
                // CharacterController ��Ȱ��ȭ
                Controller.enabled = false;

                // ��ġ ���� ����
                transform.position = Vector3.Lerp(transform.position, serverPos.Value, Time.deltaTime / 0.2f);

                // CharacterController �ٽ� Ȱ��ȭ
                Controller.enabled = true;
            }

            else
            {
                // ���� ��� �̵�
                Controller.Move(serverMove.Value);
            }

            // ȸ��
            transform.rotation = Quaternion.Lerp(transform.rotation, serverRot.Value, Time.deltaTime / 0.2f);
            
            // �ִϸ��̼�
            m_Animator.SetFloat("Move", serverAnimMoveBlend.Value);
        }
    }

    void LateUpdate()
    {
        // ���콺 �Է�
        InputLook = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        CameraRotation();
    }

    // ������
    void CharMove()
    {
        // ��ǲ�ý��ۿ��� Vector2�� ��������
        Move = InputMove;


        // �ӵ� ����
        float targetSpeed = 0.0f;

        // �Է��� ������� ��� �ӵ��� ����
        if (Move != Vector2.zero)
        {
            targetSpeed = MoveSpeed;
        }
        else
            targetSpeed = 0.0f;

        Speed = Mathf.Lerp(Speed, targetSpeed, Time.deltaTime * SpeedChangeRate);
        Speed = Mathf.Round(Speed * 1000f) / 1000f;


        // ��ֶ�����
        Vector3 inputDirection = new Vector3(Move.x, 0.0f, Move.y).normalized;

        //�̵� �Է��� �ִ� ��� �÷��̾ �̵��� �� ȸ��
        if (Move != Vector2.zero)
        {
            TargetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              Camera.main.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, TargetRotation, ref RotationVelocity,
                RotationSmoothTime);


            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, TargetRotation, 0.0f) * Vector3.forward;

        // �̵�
        Controller.Move(targetDirection.normalized * (Speed * Time.deltaTime));

        // �ִϸ��̼�
        if (m_Animator)
        {
            AnimationMoveBlend = Mathf.Lerp(AnimationMoveBlend, Move != Vector2.zero ? 1 : 0, Time.deltaTime * SpeedChangeRate);
            if (AnimationMoveBlend < 0.01f) AnimationMoveBlend = 0f;

            m_Animator.SetFloat("Move", AnimationMoveBlend);
        }

        // ������ ���°� ����
        SendStateRpc
        (
            transform.position,
            targetDirection.normalized * (Speed * Time.deltaTime),
            transform.rotation,
            AnimationMoveBlend
        );
    }

    // ī�޶� ȸ��
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

        for (int i = 0; i < UI_ObjPool.Inst.Interact_UI_List.Count; i++)
        {
            if (interaction == UI_ObjPool.Inst.Interact_UI_List[i].Get_interaction)
            {
                UI_ObjPool.Inst.Interact_UI_List[i].gameObject.SetActive(false);
                break;
            }
        }

        InteractionList.Remove(interaction);
    }

    // ������ �� ����
    [Rpc(SendTo.Server)]
    void SendStateRpc(Vector3 pos, Vector3 move, Quaternion rot, float animMoveBlend)
    {
        // ��ġ�� ����
        serverPos.Value = pos;

        // �̵��� ����
        serverMove.Value = move;

        // ȸ���� ����
        serverRot.Value = rot;

        // �ִϸ��̼� ����
        serverAnimMoveBlend.Value = animMoveBlend;
    }
}

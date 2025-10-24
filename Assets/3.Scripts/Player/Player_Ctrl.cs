using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class Player_Ctrl : NetworkBehaviour
{
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

    #region  ī�޶�
    // ī�޶�
    GameObject CameraTargetRoot;        // ī�޶� �ٶ� Ÿ��

    float CinemachineTargetYaw = 0;
    float CinemachineTargetPitch = 0;

    float TopClamp = 70.0f;
    float BottomClamp = -20.0f;
    #endregion

    #region  ��ȣ�ۿ�
    // ��ȣ�ۿ� ������
    List<Interaction_UI> InteractionUI_List = new List<Interaction_UI>();
    public int ChangeInteractionCount = 0;
    #endregion


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
        // ���� �Է�
        InputMove = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        CharMove();

        if (Input.GetKeyDown(KeyCode.F))
        {
            Interaction();
        }

        // ���콺 ��
        MouseScroll(); 
        
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


    // ��ȣ�ۿ�
    void Interaction()
    {
        if (InteractionUI_List.Count <= 0) return;

        InteractionUI_List[ChangeInteractionCount].Interaction();

        // ����
        Destroy(InteractionUI_List[ChangeInteractionCount].gameObject);
        InteractionUI_List.RemoveAt(ChangeInteractionCount);

        // ���õ� ��ȣ�ۿ� ���� ����
        if (InteractionUI_List.Count < ChangeInteractionCount)
            ChangeInteractionCount = InteractionUI_List.Count;
    }

    // ���콺 ��
    void MouseScroll()
    {
        if (Input.GetAxis("Mouse ScrollWheel") == 0) return;

        ChangeInteraction(Input.GetAxis("Mouse ScrollWheel"));
    }

    // ��ȣ�ۿ� ���콺 �ٷ� ����
    void ChangeInteraction(float scroll)
    {
        if(InteractionUI_List.Count == 0) return;

        // �� Ű�ٿ�
        if (scroll == 0) return;

        if (scroll > 0) // ���콺 �� �� (������ ��ũ��)
        {
            ChangeInteractionCount--;

            if (ChangeInteractionCount < 0)
                ChangeInteractionCount = InteractionUI_List.Count - 1;
        }
        else if (scroll < 0) // ���콺 �� �ٿ� (�ڷ� ��ũ��)
        {
            ChangeInteractionCount++;

            if (InteractionUI_List.Count <= ChangeInteractionCount)
                ChangeInteractionCount = 0;
        }


        for(int i = 0; i < InteractionUI_List.Count; i++)
        {
            InteractionUI_List[i].Change(i == ChangeInteractionCount);
        }

    }


    private void OnTriggerEnter(Collider other)
    {
        // ��ȣ�ۿ� ������Ʈ�� �ƴ϶�� ����
        if (other.tag != "Interaction") return;
        Interaction interaction = other.gameObject.GetComponent<Interaction>();
        if (interaction == null) return;


        // �ߺ� üũ
        bool isDuplicate = false;
        for (int i = 0; i < InteractionUI_List.Count; i++)
        {
            if (InteractionUI_List[i].InteractionType != interaction.InteractionType) continue;

            switch(InteractionUI_List[i].InteractionType)
            {
                // ������ Ÿ��
                case EInteractionType.Itme:
                    {
                        Interaction_Item interaction_Item = interaction.GetComponent<Interaction_Item>();

                        // ������ �ڵ尡 ���� �������� ��� ��ġ��
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

        if(isDuplicate == false)
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
        // ��ȣ�ۿ� ������Ʈ�� �ƴ϶�� ����
        if (other.tag != "Interaction") return;
        Interaction interaction = other.gameObject.GetComponent<Interaction>();
        if (interaction == null) return;


        bool isDestroy = false;
        foreach (Interaction_UI ui in InteractionUI_List)
        {
            // ��ȣ�ۿ� ������Ʈ�� UI�� ���� Ÿ���� �ƴ϶�� �ѱ��
            if (ui.InteractionType != interaction.InteractionType) continue;

            switch (ui.InteractionType)
            {
                // ������ Ÿ��
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

        // ���� ���� �ߴٸ�
        if(isDestroy)
        {
            // ���õ� ��ȣ�ۿ� ���� ����
            if (InteractionUI_List.Count < ChangeInteractionCount)
                ChangeInteractionCount = InteractionUI_List.Count;
        }

        for (int i = 0; i < InteractionUI_List.Count; i++)
        {
            InteractionUI_List[i].Change(i == ChangeInteractionCount);
        }
    }


}

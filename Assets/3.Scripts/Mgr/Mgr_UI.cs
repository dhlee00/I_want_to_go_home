using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Mgr_UI : MonoBehaviour
{
    public static Mgr_UI Inst;

    [SerializeField] Transform UI_Parent;

    #region Inventory
    [Header("Inventory")]
    [SerializeField] GameObject Inventory_Prefab;
    GameObject Inventory_UI;
    bool IsInventory_UI;

    public bool OnInventory() { return IsInventory_UI = Spawn_UI(Inventory_Prefab, Inventory_UI); }
    
    // 인벤토리가 열려도 괜찮은 조건인지 확인
    public bool IsCanOnInventory()
    {
        return !Storage_UI;
    }
    #endregion

    #region EquipSlot_Info
    [Header("EquipSlot_Info")]
    [SerializeField] Animator EquipSlot_Anim;
    public List<EquipSlot_Info> EquipSlotInfo_List = new List<EquipSlot_Info>();
    #endregion

    #region Pointer
    [Header("Pointer")]
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;
    #endregion

    #region
    [Header("Player Stets UI")]
    [SerializeField] Image HpBar_Image;
    [SerializeField] Image HungerBar_Image;
    [SerializeField] Image ThirstBar_Image;
    [SerializeField] Image StaminaBar_Image;


    #endregion

    #region CraftingStation
    [Header("CraftingStation")]
    [SerializeField] UI_CraftingStation CraftingStation_Obj;
    bool IsCraftingStation_UI;
    #endregion

    #region Storage
    [Header("Storage")]
    [SerializeField] Storage_UI Storage_Obj;
    bool Storage_UI;
    #endregion

    #region IceMelter_UI
    [Header("IceMelter_UI")]
    [SerializeField] UI_IceMelter IceMelter_Obj;
    bool bIceMelter_UI;
    #endregion

    #region WaterOxidizer_UI
    [Header("WaterOxidizer_UI")]
    [SerializeField] UI_WaterOxidizer WaterOxidizer_Obj;
    bool bIceWaterOxidizer_UI;
    #endregion


    void Start()
    {
        #region Singleton
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(this);
        }
        #endregion  

        Init_UI(Inventory_Prefab, ref Inventory_UI);

        // 시작시 제작대 끄기
        CraftingStation(false);
    }

    void Update()
    {
        if (Player_Ctrl.LocalPlayer && HpBar_Image && StaminaBar_Image)
        {
            // 스텟
            {
                // 체력
                HpBar_Image.fillAmount = Player_Ctrl.LocalPlayer.Hp / Player_Ctrl.LocalPlayer.Max_Hp;
                
                // 포만감
                HungerBar_Image.fillAmount = Player_Ctrl.LocalPlayer.Current_Hunger / Player_Ctrl.LocalPlayer.Max_Hunger;

                // 수분
                ThirstBar_Image.fillAmount = Player_Ctrl.LocalPlayer.Current_Thirst / Player_Ctrl.LocalPlayer.Max_Thirst;

            }

            if (Player_Ctrl.LocalPlayer.Current_Stamina >= Player_Ctrl.LocalPlayer.Max_Stamina - 0.01f)
            {
                StaminaBar_Image.gameObject.SetActive(false);
            }
            else
            {
                StaminaBar_Image.gameObject.SetActive(true);
                StaminaBar_Image.fillAmount = Player_Ctrl.LocalPlayer.Current_Stamina / Player_Ctrl.LocalPlayer.Max_Stamina;
            }
        }
    }

    // UI가 켜져있는지 체크하여 bool로 리턴
    public bool IsUIActive() { return IsInventory_UI || IsCraftingStation_UI; }



    #region Spawn_UI
    bool Spawn_UI(GameObject _uiPrefab, GameObject _ui)
    {
        bool isOn = false;
        // UI 생성 되있고 비횔성화 중이면 (열기)
        if (_ui != null && _ui.activeSelf == false)
        {
            Inventory_UI.SetActive(true);
            isOn = true;
        }
        // UI 생성 되있고 횔성화 중이면 (닫기)
        else if (_ui != null && _ui.activeSelf == true)
        {
            _ui.GetComponent<Animator>().Play("Close");
            isOn = false;
        }

        return isOn;
    }

    void Init_UI(GameObject _uiPrefab, ref GameObject _ui)
    {
        // UI 생성
        if (_ui == null)
        {
            GameObject spawnUI = Instantiate(_uiPrefab);
            _ui = spawnUI;
            spawnUI.transform.SetParent(UI_Parent, false);
            spawnUI.SetActive(false);
        }
    }
    #endregion

    #region Interact_UI
    List<Interaction_UI> InteractionUI_List = new List<Interaction_UI>();
    int ChangeInteractionCount = 0;


    public void Interaction()
    {
        if (InteractionUI_List.Count <= 0) return;

        // 상호작용
        InteractionUI_List[ChangeInteractionCount].Interaction();


        switch (InteractionUI_List[ChangeInteractionCount].InteractionType)
        {
            case EInteractionType.item:
                {
                    // 삭제
                    Destroy(InteractionUI_List[ChangeInteractionCount].gameObject);
                    InteractionUI_List.Remove(InteractionUI_List[ChangeInteractionCount]);

                    if (ChangeInteractionCount >= InteractionUI_List.Count)// 마지막 인덱스를 상호작용했을때
                    {
                        // 선택된 상호작용 순서 변경
                        ChangeInteraction(true);
                    }
                    else
                    {
                        // UI 업데이트
                        InteractionUI_Update();
                    }
                    break;
                }

            case EInteractionType.door:
                {
                    // UI 업데이트
                    for (int i = 0; i < InteractionUI_List.Count; i++)
                    {
                        if (InteractionUI_List[i].InteractionType == EInteractionType.door)
                        {
                            InteractionUI_List[i].UI_Update();
                        }
                    }
                }
                break;
        }

        Mgr_UI.Inst.EquipInfo_Init();
    }

    // 선택 중인 상호작용UI 설정
    public void ChangeInteraction(bool bUp)
    {
        if (InteractionUI_List.Count == 0) return;


        if (bUp) // 위로
        {
            ChangeInteractionCount--;

            if (ChangeInteractionCount < 0)
                ChangeInteractionCount = InteractionUI_List.Count - 1;
        }
        else //아래
        {
            ChangeInteractionCount++;

            if (ChangeInteractionCount >= InteractionUI_List.Count)
                ChangeInteractionCount = 0;
        }

        // UI 업데이트
        InteractionUI_Update();

    }


    public void AddInteractionUI(Interaction interaction)
    {
        // 중복 체크
        bool isDuplicate = false;

        switch (interaction.InteractionType)
        {

            // 아이템 타입
            case EInteractionType.item:
                {
                    if (interaction is Interaction_Item item)
                    {
                        for (int i = 0; i < InteractionUI_List.Count; i++)
                        {
                            if (InteractionUI_List[i].Item_Obj_List.Count == 0) continue;

                            // 아이템 코드가 같은 아이템일 경우 합치기
                            if (InteractionUI_List[i].Item_Obj_List[0].ItemData.Get_Item_Index == item.ItemData.Get_Item_Index)
                            {
                                InteractionUI_List[i].Item_Obj_List.Add(item);
                                InteractionUI_List[i].UI_Update();
                                isDuplicate = true;
                            }
                        }
                    }
                    break;
                }

            default:
                {
                    for (int i = 0; i < InteractionUI_List.Count; i++)
                    {
                        if (InteractionUI_List[i] == isDuplicate)
                            isDuplicate = true;
                    }

                    break;
                }
        }



        if (isDuplicate == false)
        {
            InteractionUI_List.Add(Mgr_UI.Inst.Spawn_Interaction_UI(interaction));

            InteractionUI_Update();
        }
    }

    public void RemoveInteractionUI(Interaction interaction)
    {
        bool isDestroy = false;
        foreach (Interaction_UI ui in InteractionUI_List)
        {
            // 상호작용 오브젝트와 UI가 같은 타입이 아니라면 넘기기
            if (ui.InteractionType != interaction.InteractionType) continue;

            switch (ui.InteractionType)
            {
                // 아이템 타입
                case EInteractionType.item:
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

                default:
                    {
                        InteractionUI_List.Remove(ui);
                        Destroy(ui.gameObject);
                        isDestroy = true;
                        break;
                    }
            }

            if (isDestroy)
                break;
        }

        // 만약 삭제 했다면
        if (isDestroy && ChangeInteractionCount >= InteractionUI_List.Count)
            ChangeInteraction(true);

        InteractionUI_Update();
    }


    // 어떤 상호작용을 선택중인지 업데이트
    void InteractionUI_Update()
    {
        for (int i = 0; i < InteractionUI_List.Count; i++)
        {
            InteractionUI_List[i].Change(i == ChangeInteractionCount);
        }
    }

    public Interaction_UI Spawn_Interaction_UI(Interaction interaction)
    {
        GameObject obj = Instantiate(UI_ObjPool.Inst.Get_Interaction_UI_Prefab.gameObject);
        Interaction_UI interaction_UI = obj.GetComponent<Interaction_UI>();
        obj.transform.SetParent(UI_ObjPool.Inst.Get_Interact_UI_Tr, false);


        interaction_UI.InteractionType = interaction.InteractionType; // 타입복사

        switch (interaction.InteractionType)
        {
            case EInteractionType.item:
                {
                    interaction_UI.Item_Obj_List.Add(interaction.GetComponent<Interaction_Item>());

                    break;
                }

            default:
                {
                    interaction_UI.Interaction_Obj = interaction;
                    break;
                }
        }


        interaction_UI.UI_Update();

        return interaction_UI;
    }
    #endregion

    #region EquipSlot_On/Off
    public void EquipSlot_On(bool _isOn)
    {
        if (_isOn)
        {
            EquipSlot_Anim.Play("EquipSlot_Open");
        }
        else
        {
            EquipSlot_Anim.Play("EquipSlot_Close");
        }
    }
    #endregion

    #region SceneEquipSlot_Init
    // 화면에 보이는 장착 슬롯 초기화(수량, 정보, 슬롯 위치 초기화)
    public void EquipInfo_Init()
    {
        for (int i = 0; i < EquipSlotInfo_List.Count; i++)
        {
            EquipSlotInfo_List[i].Set_UI();
        }

        foreach (var item in GlobalValue.User_EquipSlot)
        {
            EquipSlotInfo_List[item.Value.Get_Item_SlotIndex].Set_UI(item.Value);
        }

        for (int i = 0; i < GlobalValue.Equipment_EquipSlot.Count; i++)
        {
            EquipSlotInfo_List[GlobalValue.Equipment_EquipSlot[i].Get_Item_SlotIndex].Set_UI(GlobalValue.Equipment_EquipSlot[i]);
        }
    }
    #endregion


    // 제작대 UI
    #region CraftingStation
    public void CraftingStation(bool isOn, Interaction_CraftingStation inCraftingStationData = null)
    {
        CraftingStation_Obj.gameObject.SetActive(isOn);
        IsCraftingStation_UI = isOn;

        if (!isOn || inCraftingStationData == null) return;

        // 제작대 셋팅
        CraftingStation_Obj.SetUICraftingStation(inCraftingStationData);
    }
    #endregion

    // 창고 UI
    #region Storage
    public void Storage(bool isOn, int _storageSlotCount, Storage _storage = null)
    {
        Storage_Obj.gameObject.SetActive(isOn);
        Storage_UI = isOn;

        //켰을때
        if (isOn)
        {
            // 업그레이드 되서 슬롯 개수가 달라지면
            if (_storageSlotCount != Storage_Obj.Get_StorageSlotList.Count)
            {
                int makeCout = _storageSlotCount - Storage_Obj.Get_StorageSlotList.Count;
                for (int i = 0; i < makeCout; i++)
                {
                    Storage_Obj.MakeStorageSlot();
                }
            }

            Storage_Obj.Refresh_StorageInven();
        }

        if (!isOn || _storage == null) return;
    }
    #endregion

    #region 가열기
    public void IceMelterUI(bool isOn)
    {
        IceMelter_Obj.gameObject.SetActive(isOn);
        bIceMelter_UI = isOn;

        IceMelter_Obj.UIUpdate();
    }
    #endregion

    #region 수전해 장치
    public void IceWaterOxidizerUI(bool isOn)
    {
        WaterOxidizer_Obj.gameObject.SetActive(isOn);
        bIceWaterOxidizer_UI = isOn;

        WaterOxidizer_Obj.UIUpdate();
    }
    #endregion
}

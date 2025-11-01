using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Mgr_UI : MonoBehaviour
{
    public static Mgr_UI Inst;

    [SerializeField] Transform UI_Parent;

    [Header("Inventory")]
    [SerializeField] GameObject Inventory_Prefab;
    GameObject Inventory_UI;

    [Header("Pointer")]
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    public bool OnInventory() { return Spawn_UI(Inventory_Prefab, Inventory_UI); }

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
    }


    void Update()
    {

    }

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
    public int ChangeInteractionCount = 0;


    public void Interaction()
    {
        if (InteractionUI_List.Count <= 0) return;

        InteractionUI_List[ChangeInteractionCount].Interaction();

        // 삭제
        {
            Destroy(InteractionUI_List[ChangeInteractionCount].gameObject);
            InteractionUI_List.Remove(InteractionUI_List[ChangeInteractionCount]);
        }



        if (ChangeInteractionCount >= InteractionUI_List.Count)// 마지막 인덱스를 상호작용했을때
        {
            // 선택된 상호작용 순서 변경
            ChangeInteraction(true);
        }
        else
        {
            // UI 업데이트
            for (int i = 0; i < InteractionUI_List.Count; i++)
                InteractionUI_List[i].Change(i == ChangeInteractionCount);
        }

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
        for (int i = 0; i < InteractionUI_List.Count; i++)
            InteractionUI_List[i].Change(i == ChangeInteractionCount);

    }


    public void AddInteractionUI(Interaction interaction)
    {
        // 중복 체크
        bool isDuplicate = false;


        if (interaction.InteractionType == EInteractionType.item &&
            interaction is Interaction_Item item)
        {
            if (item.ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
            {
                for (int i = 0; i < InteractionUI_List.Count; i++)
                {
                    switch (InteractionUI_List[i].InteractionType)
                    {
                        // 아이템 타입
                        case EInteractionType.item:
                            {
                                // 아이템 코드가 같은 아이템일 경우 합치기
                                if (InteractionUI_List[i].Item_Obj_List[0].ItemData.Get_Item_Index == item.ItemData.Get_Item_Index)
                                {
                                    InteractionUI_List[i].Item_Obj_List.Add(item);
                                    InteractionUI_List[i].UI_Update();
                                    isDuplicate = true;
                                }
                                break;
                            }
                    }
                }
            }
        }


        if (isDuplicate == false)
        {
            InteractionUI_List.Add(Mgr_UI.Inst.Spawn_Interaction_UI(interaction));

            for (int i = 0; i < InteractionUI_List.Count; i++)
            {
                InteractionUI_List[i].Change(i == ChangeInteractionCount);
            }
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
            }

            if (isDestroy)
                break;
        }

        // 만약 삭제 했다면
        if (isDestroy && ChangeInteractionCount >= InteractionUI_List.Count)
            ChangeInteraction(true);
    }

    public Interaction_UI Spawn_Interaction_UI(Interaction interaction)
    {
        GameObject obj = Instantiate(UI_ObjPool.Inst.Get_Interaction_UI_Prefab.gameObject);
        Interaction_UI interaction_UI = obj.GetComponent<Interaction_UI>();
        obj.transform.SetParent(UI_ObjPool.Inst.Get_Interact_UI_Tr, false);


        switch (interaction.InteractionType)
        {
            case EInteractionType.item:
                {
                    interaction_UI.Item_Obj_List.Add(interaction.GetComponent<Interaction_Item>());

                    break;
                }
        }

        interaction_UI.InteractionType = interaction.InteractionType;
        interaction_UI.UI_Update();

        return interaction_UI;
    }
    #endregion
}

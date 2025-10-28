using System.Collections.Generic;
using UnityEngine;

public class UI_ObjPool : MonoBehaviour
{
    [Header("Interact_UI")]
    [SerializeField] Interaction_UI Interact_UI_Prefab;
    public Interaction_UI Get_Interaction_UI_Prefab { get => Interact_UI_Prefab; }
    [SerializeField] Transform Interact_UI_Tr;
    public Transform Get_Interact_UI_Tr { get => Interact_UI_Tr; }

    public static UI_ObjPool Inst = null;

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
    }


    //#region 상호작용 UI
    //List<Interaction_UI> InteractionUI_List = new List<Interaction_UI>();
    //public int ChangeInteractionCount = 0;


    //public void Interaction()
    //{
    //    if (InteractionUI_List.Count <= 0) return;

    //    InteractionUI_List[ChangeInteractionCount].Interaction();

    //    // 삭제
    //    {
    //        Destroy(InteractionUI_List[ChangeInteractionCount].gameObject);
    //        InteractionUI_List.Remove(InteractionUI_List[ChangeInteractionCount]);
    //    }
        

        
    //    if(ChangeInteractionCount >= InteractionUI_List.Count)// 마지막 인덱스를 상호작용했을때
    //    {
    //        // 선택된 상호작용 순서 변경
    //        ChangeInteraction(true);
    //    }
    //    else 
    //    {
    //        // UI 업데이트
    //        for (int i = 0; i < InteractionUI_List.Count; i++)
    //            InteractionUI_List[i].Change(i == ChangeInteractionCount);
    //    }
        
    //}

    //// 선택 중인 상호작용UI 설정
    //public void ChangeInteraction(bool bUp)
    //{
    //    if (InteractionUI_List.Count == 0) return;
        

    //    if (bUp) // 위로
    //    {
    //        ChangeInteractionCount--;

    //        if (ChangeInteractionCount < 0)
    //            ChangeInteractionCount = InteractionUI_List.Count - 1;
    //    }
    //    else //아래
    //    {
    //        ChangeInteractionCount++;

    //        if (ChangeInteractionCount >= InteractionUI_List.Count)
    //            ChangeInteractionCount = 0;
    //    }

    //    // UI 업데이트
    //    for (int i = 0; i < InteractionUI_List.Count; i++)
    //        InteractionUI_List[i].Change(i == ChangeInteractionCount);

    //}


    //public void AddInteractionUI(Interaction interaction)
    //{
    //    // 중복 체크
    //    bool isDuplicate = false;

        
    //    if(interaction.InteractionType == EInteractionType.item &&
    //        interaction is Interaction_Item item)
    //    {
    //        if(item.ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
    //        {
    //            for (int i = 0; i < InteractionUI_List.Count; i++)
    //            {
    //                switch (InteractionUI_List[i].InteractionType)
    //                {
    //                    // 아이템 타입
    //                    case EInteractionType.item:
    //                        {
    //                            // 아이템 코드가 같은 아이템일 경우 합치기
    //                            if (InteractionUI_List[i].Item_Obj_List[0].ItemData.Get_Item_Index == item.ItemData.Get_Item_Index)
    //                            {
    //                                InteractionUI_List[i].Item_Obj_List.Add(item);
    //                                InteractionUI_List[i].UI_Update();
    //                                isDuplicate = true;
    //                            }
    //                            break;
    //                        }
    //                }
    //            }
    //        }
    //    }
        

    //    if (isDuplicate == false)
    //    {
    //        InteractionUI_List.Add(UI_ObjPool.Inst.Spawn_Interaction_UI(interaction));

    //        for (int i = 0; i < InteractionUI_List.Count; i++)
    //        {
    //            InteractionUI_List[i].Change(i == ChangeInteractionCount);
    //        }
    //    }

        
    //}

    //public void RemoveInteractionUI(Interaction interaction)
    //{
    //    bool isDestroy = false;
    //    foreach (Interaction_UI ui in InteractionUI_List)
    //    {
    //        // 상호작용 오브젝트와 UI가 같은 타입이 아니라면 넘기기
    //        if (ui.InteractionType != interaction.InteractionType) continue;

    //        switch (ui.InteractionType)
    //        {
    //            // 아이템 타입
    //            case EInteractionType.item:
    //                {
    //                    Interaction_Item interaction_Item = interaction.GetComponent<Interaction_Item>();

    //                    ui.Item_Obj_List.Remove(interaction_Item);
    //                    ui.UI_Update();

    //                    if (ui.Item_Obj_List.Count <= 0)
    //                    {
    //                        InteractionUI_List.Remove(ui);
    //                        Destroy(ui.gameObject);

    //                        isDestroy = true;
    //                    }
    //                    break;
    //                }
    //        }

    //        if (isDestroy)
    //            break;
    //    }

    //    // 만약 삭제 했다면
    //    if (isDestroy && ChangeInteractionCount >= InteractionUI_List.Count)
    //        ChangeInteraction(true);
    //}

    //public Interaction_UI Spawn_Interaction_UI(Interaction interaction)
    //{
    //    GameObject obj = Instantiate(Interact_UI_Prefab.gameObject);
    //    Interaction_UI interaction_UI = obj.GetComponent<Interaction_UI>();
    //    obj.transform.SetParent(Interact_UI_Tr, false);


    //    switch (interaction.InteractionType)
    //    {
    //        case EInteractionType.item:
    //            {
    //                interaction_UI.Item_Obj_List.Add(interaction.GetComponent<Interaction_Item>());
                    
    //                break;
    //            }
    //    }

    //    interaction_UI.InteractionType = interaction.InteractionType;
    //    interaction_UI.UI_Update();

    //    return interaction_UI;
    //}
    //#endregion
}

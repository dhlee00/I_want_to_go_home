using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public enum SLOT_TYPE
{
    EQUIP,
    INVEN,
}


public class Inven_Slot : MonoBehaviour, ISlot, 
    IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] SLOT_TYPE SlotType;
    public SLOT_TYPE Get_SlotType { get => SlotType; }

    [SerializeField] int SlotNum;
    [SerializeField] Image Item_Icon;
    [SerializeField] TextMeshProUGUI Item_Amount;
    [SerializeField] Item ItemData;
    public bool isUse;

    public Item Get_ItemData { get => ItemData; }
    public bool Get_isUse { get => isUse; }
    public int Get_SlotNum { get => SlotNum; }

    // 슬롯 인덱스 부여
    void ISlot.Set_SlotNum(int num)
    {
        SlotNum = num;
    }

    public void Set_SlotType(SLOT_TYPE _slotType)
    {
        SlotType = _slotType;
    }

    // 슬롯 아이템 정보 설정
    public void Set_SlotInfo(Item _item = null, int _amount = 0, bool _isUse = true)
    {
        if (_item != null)
        {
            ItemData = _item;
            Item_Icon.sprite = _item.Get_Item_Icon;
            isUse = _isUse;

            if (_item.Get_ItemType != ITEM_TYPE.EQUIPMENT && _isUse)
            {
                Item_Amount.text = $"x{_amount}";
            }
        }
        else
        {
            ItemData = _item;
            Item_Icon.sprite = null;
            isUse = _isUse;
        }

        if (_isUse == false || _item.Get_ItemType == ITEM_TYPE.EQUIPMENT)
        {
            Item_Amount.text = "";
        }
    }

    // 아이템 슬롯 변경
    #region Change_Item_Slot
    Item DragItem = null;
    public bool isDrag = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (ItemData == null || ItemData.Get_ItemType == ITEM_TYPE.NONE)
            return;

        isDrag = true;
        DragItem = ItemData;

        Mgr_Inventory.Inst.Get_DragItem.gameObject.SetActive(true);
        Mgr_Inventory.Inst.Get_DragItem.gameObject.transform.position = eventData.position;
        Mgr_Inventory.Inst.Get_DragItem.sprite = DragItem.Get_Item_Icon;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Mgr_Inventory.Inst.Get_DragItem.gameObject.transform.position = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (ItemData == null || isDrag == false)
        {
            return;
        }

        isDrag = false;
        Mgr_Inventory.Inst.Get_DragItem.gameObject.SetActive(false);

        // Raycast 준비
        PointerEventData pointerData = new PointerEventData(Mgr_UI.Inst.eventSystem);
        pointerData.position = eventData.position;

        // 레이캐스트로 받은 리스트 추가
        List<RaycastResult> results = new List<RaycastResult>();
        Mgr_UI.Inst.raycaster.Raycast(pointerData, results);

        // Raycast로 받은게 아무것도 없다면
        if (results.Count <= 0)
        {
            Mgr_Game.Inst.DropItem(ItemData);

            // 인벤토리에서 데이터 지움
            if (DragItem.Get_ItemType != ITEM_TYPE.EQUIPMENT)
            {
                GlobalValue.User_Inventory.Remove(DragItem.Get_Item_Index);
            }
            else
            {
                GlobalValue.Equipment_Inventory.Remove(DragItem);
            }

            // 인벤토리에서 지움
            Set_SlotInfo(null, 0, false);

            return;
        }

        foreach(var result in results)
        {
            // 인벤 슬롯 위치 변경
            #region SlotChange
            if (result.gameObject.GetComponent<Inven_Slot>() && 
                result.gameObject.GetComponent<Inven_Slot>().Get_SlotType == SLOT_TYPE.INVEN)
            {
                // Change_Slot<Inven_Slot>(result.gameObject);
                // 원래 인벤슬롯에 놓으면 return
                if (SlotType == result.gameObject.GetComponent<Inven_Slot>().SlotType
                    && result.gameObject.GetComponent<Inven_Slot>().SlotNum ==
                    DragItem.Get_Item_SlotIndex)
                {
                    return;
                }

                // 아이템이 없는 슬롯이면
                if (result.gameObject.GetComponent<Inven_Slot>().isUse == false)
                {
                    DragItem.Get_Item_SlotIndex = result.gameObject.GetComponent<Inven_Slot>().SlotNum;
                    result.gameObject.GetComponent<Inven_Slot>().Set_SlotInfo(DragItem, DragItem.Get_Item_Amount);

                    Set_SlotInfo(null, 0, false);

                    // 장착 => 인벤 
                    if (SlotType == SLOT_TYPE.EQUIP)
                    {
                        // 장착 슬롯 삭제 => 인벤토리로 데이터 옮기기
                        if (DragItem.Get_ItemType != ITEM_TYPE.EQUIPMENT)
                        {
                            GlobalValue.User_Inventory.Add(DragItem.Get_Item_Index, DragItem);
                            GlobalValue.User_EquipSlot.Remove(DragItem.Get_Item_Index);
                        }
                        else
                        {
                            GlobalValue.Equipment_Inventory.Add(DragItem);
                            GlobalValue.Equipment_EquipSlot.Remove(DragItem);
                        }
                    }
                }
                // 사용하고 있는 슬롯이라면
                else
                {
                    Item ChangeSlot_Item = result.gameObject.GetComponent<Inven_Slot>().ItemData;
                    // 변경할 슬롯의 아이템 정보 변경
                    Set_SlotInfo(ChangeSlot_Item, ChangeSlot_Item.Get_Item_Amount);
                    result.gameObject.GetComponent<Inven_Slot>().ItemData.Get_Item_SlotIndex = SlotNum;

                    // 해당 슬롯의 아이템 정보 변경
                    DragItem.Get_Item_SlotIndex = result.gameObject.GetComponent<Inven_Slot>().SlotNum;
                    result.gameObject.GetComponent<Inven_Slot>().Set_SlotInfo(DragItem, DragItem.Get_Item_Amount);

                    if (SlotType == SLOT_TYPE.EQUIP)
                    {
                        // 장착 -> 인벤
                        if (ChangeSlot_Item.Get_ItemType != ITEM_TYPE.EQUIPMENT)
                        {
                            // 바꾸는 아이템이 장비라면
                            if (DragItem.Get_ItemType == ITEM_TYPE.EQUIPMENT)
                            {
                                GlobalValue.Equipment_Inventory.Add(DragItem);
                                GlobalValue.Equipment_EquipSlot.Remove(DragItem);
                            }
                            else
                            {
                                GlobalValue.User_Inventory.Add(DragItem.Get_Item_Index, DragItem);
                                GlobalValue.User_EquipSlot.Remove(DragItem.Get_Item_Index);
                            }

                            GlobalValue.User_EquipSlot.Add(ChangeSlot_Item.Get_Item_Index, ChangeSlot_Item);
                            GlobalValue.User_Inventory.Remove(ChangeSlot_Item.Get_Item_Index);
                        }
                        else
                        {
                            if (DragItem.Get_ItemType == ITEM_TYPE.EQUIPMENT)
                            {
                                GlobalValue.Equipment_Inventory.Add(DragItem);
                                GlobalValue.Equipment_EquipSlot.Remove(DragItem);
                            }
                            else
                            {
                                GlobalValue.User_Inventory.Add(DragItem.Get_Item_Index, DragItem);
                                GlobalValue.User_EquipSlot.Remove(DragItem.Get_Item_Index);
                            }

                            GlobalValue.Equipment_EquipSlot.Add(ChangeSlot_Item);
                            GlobalValue.Equipment_Inventory.Remove(ChangeSlot_Item);
                        }
                    }
                }

                break;
            }
            #endregion
            // 장착 슬롯 장착
            #region Equip_Item
            else if (result.gameObject.GetComponent<Inven_Slot>() && 
                result.gameObject.GetComponent<Inven_Slot>().Get_SlotType == SLOT_TYPE.EQUIP)
            {
                // 원래 인벤슬롯에 놓으면 return
                if (SlotType == result.gameObject.GetComponent<Inven_Slot>().SlotType 
                    && result.gameObject.GetComponent<Inven_Slot>().Get_SlotNum == DragItem.Get_Item_SlotIndex)
                {
                    return;
                }

                // 아이템이 없는 슬롯이면
                if (result.gameObject.GetComponent<Inven_Slot>().isUse == false)
                {
                    DragItem.Get_Item_SlotIndex = result.gameObject.GetComponent<Inven_Slot>().Get_SlotNum;
                    result.gameObject.GetComponent<Inven_Slot>().Set_SlotInfo(DragItem, DragItem.Get_Item_Amount);

                    Set_SlotInfo(null, 0, false);

                    if (SlotType == SLOT_TYPE.INVEN)
                    {
                        // 인벤에서 삭제 => 장착 슬롯으로 데이터 옮기기
                        if (DragItem.Get_ItemType != ITEM_TYPE.EQUIPMENT)
                        {
                            GlobalValue.User_EquipSlot.Add(DragItem.Get_Item_Index, DragItem);
                            GlobalValue.User_Inventory.Remove(DragItem.Get_Item_Index);
                        }
                        else
                        {
                            GlobalValue.Equipment_EquipSlot.Add(DragItem);
                            GlobalValue.Equipment_Inventory.Remove(DragItem);
                        }
                    }
                }
                else
                {
                    // 바꿀 슬롯에 있는 아이템 정보
                    Item ChangeSlot_Item = result.gameObject.GetComponent<Inven_Slot>().Get_ItemData;

                    // 변경할 슬롯의 아이템 정보 변경
                    Set_SlotInfo(ChangeSlot_Item, ChangeSlot_Item.Get_Item_Amount);
                    result.gameObject.GetComponent<Inven_Slot>().Get_ItemData.Get_Item_SlotIndex = SlotNum;
                    // 해당 슬롯의 아이템 정보 변경
                    DragItem.Get_Item_SlotIndex = result.gameObject.GetComponent<Inven_Slot>().Get_SlotNum;

                    result.gameObject.GetComponent<Inven_Slot>().Set_SlotInfo(DragItem, DragItem.Get_Item_Amount);

                    // 인벤 -> 장착
                    if (SlotType == SLOT_TYPE.INVEN)
                    {
                        //  바꿀 위치 아이템이 재료 아이템이라면
                        if (ChangeSlot_Item.Get_ItemType != ITEM_TYPE.EQUIPMENT)
                        {
                            // 바꾸는 아이템이 장비라면
                            if (DragItem.Get_ItemType == ITEM_TYPE.EQUIPMENT)
                            {
                                GlobalValue.Equipment_EquipSlot.Add(DragItem);
                                GlobalValue.Equipment_Inventory.Remove(DragItem);
                            }
                            else
                            {
                                GlobalValue.User_EquipSlot.Add(DragItem.Get_Item_Index, DragItem);
                                GlobalValue.User_Inventory.Remove(DragItem.Get_Item_Index);
                            }

                            GlobalValue.User_Inventory.Add(ChangeSlot_Item.Get_Item_Index, ChangeSlot_Item);
                            GlobalValue.User_EquipSlot.Remove(ChangeSlot_Item.Get_Item_Index);
                        }
                        // 바꿀 위치 아이템이 장비라면
                        else
                        {
                            if (DragItem.Get_ItemType == ITEM_TYPE.EQUIPMENT)
                            {
                                GlobalValue.Equipment_EquipSlot.Add(DragItem);
                                GlobalValue.Equipment_Inventory.Remove(DragItem);

                            }
                            else
                            {
                                GlobalValue.User_EquipSlot.Add(DragItem.Get_Item_Index, DragItem);
                                GlobalValue.User_Inventory.Remove(DragItem.Get_Item_Index);
                            }

                            GlobalValue.Equipment_Inventory.Add(ChangeSlot_Item);
                            GlobalValue.Equipment_EquipSlot.Remove(ChangeSlot_Item);
                        }
                    }
                }

                break;
            }
            #endregion
        }

        #region Test
        string debug = "재료 인벤";
        foreach(var item in GlobalValue.User_Inventory)
        {
            debug += $" {item.Value.Get_Item_Name}, ";
        }
        debug += "\n재료 장착";

        foreach (var item in GlobalValue.User_EquipSlot)
        {
            debug += $" {item.Value.Get_Item_Name}, ";
        }

        debug += "\n장비 인벤";
        for (int i = 0; i < GlobalValue.Equipment_Inventory.Count; i++)
        {
            debug += $" {GlobalValue.Equipment_Inventory[i].Get_Item_Name}, ";
        }
        debug += "\n장비 장착";
        for (int i = 0; i < GlobalValue.Equipment_EquipSlot.Count; i++)
        {
            debug += $" {GlobalValue.Equipment_EquipSlot[i].Get_Item_Name}, ";
        }
        debug += "\n";

        Debug.Log(debug);
        #endregion
        
        DragItem = null;
        Mgr_Inventory.Inst.Refresh_Inventory();
        Mgr_UI.Inst.EquipInfo_Init();
    }
    #endregion

    // 아이템 설명 UI
    #region Item_Desc
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemData == null || ItemData.Get_ItemType == ITEM_TYPE.NONE)
            return;

        Mgr_Inventory.Inst.Get_Item_Desc.gameObject.SetActive(true);
        Mgr_Inventory.Inst.Get_Item_Desc.Set_UI_Info(ItemData);

        // UI 위치
        Mgr_Inventory.Inst.Get_Item_Desc.gameObject.transform.position =
            this.gameObject.transform.position + transform.right * 245 + transform.up * -160;
           
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemData == null || ItemData.Get_ItemType == ITEM_TYPE.NONE)
            return;

        Mgr_Inventory.Inst.Get_Item_Desc.gameObject.SetActive(false);
    }

    #endregion
}

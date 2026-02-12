using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ISlot
{
    void Set_SlotNum(int num);
    void Set_SlotType(SLOT_TYPE _slotType);
}

public class Mgr_Inventory : MonoBehaviour
{
    [Header("Inven_Item_Slot")] // 가방 슬롯
    [SerializeField] List<Inven_Slot> Inven_ItemSlotList = new List<Inven_Slot>();
    public List<Inven_Slot> Get_Inven_ItemSlotList { get => Inven_ItemSlotList; }
    [SerializeField] GameObject Inven_ItemSlot_Prefab;
    [SerializeField] Transform ItemSlot_Tr;
    [SerializeField] int ItemSlot_Amount;

    [Header("Inven_Equip_Slot")] // 장착 슬롯
    [SerializeField] List<Inven_Slot> Inven_EquipSlotList = new List<Inven_Slot>();
    [SerializeField] GameObject Inven_EquipSlot_Prefab;
    [SerializeField] Transform EquipSlot_Tr;
    [SerializeField] int EquipSlot_Amount;

    [SerializeField] GameObject ArmorSlot;
    [SerializeField] GameObject EquipSlot;

    [SerializeField] Image DragItem;

    int _Equip_Slot_Index;    // 현제 장착중인 슬롯 인덱스
    public int Equip_Slot_Index 
    { get { return _Equip_Slot_Index; } set { _Equip_Slot_Index = value; UpdateEquipSlot(); } }

    public Image Get_DragItem { get => DragItem; }

    [SerializeField] Item_Desc ItemDesc;
    public Item_Desc Get_Item_Desc { get => ItemDesc; }

    public static Mgr_Inventory Inst = null;

    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }

        // 아이템 슬롯 생성
        Spawn_Slot<Inven_Slot>(ItemSlot_Amount, Inven_ItemSlot_Prefab, ItemSlot_Tr, Inven_ItemSlotList, SLOT_TYPE.INVEN);

        // 장착 슬롯 생성
        Spawn_Slot<Inven_Slot>(EquipSlot_Amount, Inven_ItemSlot_Prefab, EquipSlot_Tr, Inven_EquipSlotList, SLOT_TYPE.EQUIP);

        // 불러온 데이터 인벤토리에 추가를 위해 새로고침
        Refresh_Inventory();


        Equip_Slot_Index = 1;
    }

    // 슬롯 생성
    #region Spawn_Slot
    void Spawn_Slot<T>(int _count, GameObject _prefab, Transform _tr, List<T> _list, SLOT_TYPE _slotType)
        where T : Component, ISlot
    {
        for (int i = 0; i < _count; i++)
        {
            GameObject slot = Instantiate(_prefab);
            slot.transform.SetParent(_tr);
            T comp = slot.GetComponent<T>();
            comp.Set_SlotNum(i);
            comp.Set_SlotType(_slotType);
            _list.Add(comp);
        }
    }
    #endregion

    #region Refresh Inventory
    public void Refresh_Inventory()
    {
        // int index = 0;
        foreach(var item in GlobalValue.User_Inventory)
        {
            Inven_ItemSlotList[item.Value.Get_Item_SlotIndex].Set_SlotInfo(item.Value, item.Value.Get_Item_Amount);
        }

        foreach(var item in GlobalValue.Equipment_Inventory)
        {
            Inven_ItemSlotList[item.Get_Item_SlotIndex].Set_SlotInfo(item, item.Get_Item_Amount);
        }

        foreach (var item in GlobalValue.User_EquipSlot)
        {
            Inven_EquipSlotList[item.Value.Get_Item_SlotIndex].Set_SlotInfo(item.Value, item.Value.Get_Item_Amount);
        }

        foreach (var item in GlobalValue.Equipment_EquipSlot)
        {
            Inven_EquipSlotList[item.Get_Item_SlotIndex].Set_SlotInfo(item, item.Get_Item_Amount);
        }
    }
    #endregion

    #region Change_ArmorSlot
    public void Change_ArmorSlot(bool _isOn)
    {
        if (_isOn)
        {
            ArmorSlot.SetActive(_isOn);
            EquipSlot.SetActive(!_isOn);
        }
        else
        {
            ArmorSlot.SetActive(_isOn);
            EquipSlot.SetActive(!_isOn);
        }
    }
    #endregion


    public void UpdateEquipSlot()
    {
        Player_Ctrl.LocalPlayer.EquipWeapon((Inven_EquipSlotList[Equip_Slot_Index - 1].Get_ItemData == null) ? "" : Inven_EquipSlotList[Equip_Slot_Index - 1].Get_ItemData?.Get_Item_Prefab);

        for(int i = 0; i < Mgr_UI.Inst.EquipSlotInfo_List.Count; i++)
        {
            Mgr_UI.Inst.EquipSlotInfo_List[i].isSelect(i == Equip_Slot_Index - 1);
        }

        
    }


    // 보유 아이템 갯수 리턴
    public int FindInventoryItme(int inItemIndex)
    {
        int amount = 0;

        foreach(Inven_Slot slot in Inven_ItemSlotList)
        {
            if(slot.Get_ItemData != null && slot.Get_ItemData.Get_Item_Index == inItemIndex)
            {
                amount += slot.Get_ItemData.Get_Item_Amount;
            }
        }

        foreach (Inven_Slot slot in Inven_EquipSlotList)
        {
            if (slot.Get_ItemData != null && slot.Get_ItemData.Get_Item_Index == inItemIndex)
            {
                amount += slot.Get_ItemData.Get_Item_Amount;
            }
        }

        return amount;
    }

    // 보유 아이템 사용
    public void UseInventoryItem(int inIndex, int inUseAmount)
    {
        for (int i = 0; i < Inven_ItemSlotList.Count; i++)
        {
            if (Inven_ItemSlotList[i].Get_ItemData != null && Inven_ItemSlotList[i].Get_ItemData.Get_Item_Index == inIndex)
            {
                Inven_ItemSlotList[i].UseItem(inUseAmount);

                // 보유 갯수가 0보다 작다면 인벤토리에서 삭제
                if (Inven_ItemSlotList[i].Get_ItemData.Get_Item_Amount <= 0)
                {
                    GlobalValue.User_Inventory.Remove(Inven_ItemSlotList[i].Get_ItemData.Get_Item_Index);
                    Inven_ItemSlotList[i].Set_SlotInfo(null, 0, false);
                }
                    
            }
        }

        for (int i = 0; i < Inven_EquipSlotList.Count; i++)
        {
            if (Inven_EquipSlotList[i].Get_ItemData != null && Inven_EquipSlotList[i].Get_ItemData.Get_Item_Index == inIndex)
            {
                Inven_EquipSlotList[i].UseItem(inUseAmount);

                // 보유 갯수가 0보다 작다면 인벤토리에서 삭제
                if(Inven_EquipSlotList[i].Get_ItemData.Get_Item_Amount <= 0)
                {
                    GlobalValue.Equipment_Inventory.Remove(Inven_EquipSlotList[i].Get_ItemData);
                    Inven_EquipSlotList[i].Set_SlotInfo(null, 0, false);
                }
                
            }
        }

        
        Refresh_Inventory();
    }
}

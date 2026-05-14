using System.Collections.Generic;
using UnityEngine;

public class Storage_UI : MonoBehaviour
{
    [Header("Storage")]

    [SerializeField] GameObject StorageSlot;
    [SerializeField] Transform StorageSlot_Tr;
    [SerializeField] List<Storage_Slot> StorageSlot_List = new List<Storage_Slot>();
    public List<Storage_Slot> Get_StorageSlotList { get => StorageSlot_List; }

    [Header("Inven")]
    [SerializeField] GameObject InvenSlot;
    [SerializeField] Transform InvenSlot_Tr;
    [SerializeField] List<Storage_Slot> Inven_ItemSlotList = new List<Storage_Slot>();
    public List<Storage_Slot> Get_Inven_ItemSlotList { get => Inven_ItemSlotList; }

    void Start()
    {
        for (int i = 0; i < Mgr_Inventory.Inst.Get_ItemSlot_Amount; i++)
        {
            GameObject slot = Instantiate(InvenSlot, InvenSlot_Tr);
            slot.GetComponent<Storage_Slot>().Get_SlotNum = i;
            Inven_ItemSlotList.Add(slot.GetComponent<Storage_Slot>());
        }
    }

    public void Close_UI()
    {
        Mgr_Game.Inst.OpenStorageUI(false);
    }

    // 창고 슬롯 확장
    public void MakeStorageSlot()
    {
        GameObject slot = Instantiate(StorageSlot, StorageSlot_Tr);
        StorageSlot_List.Add(slot.GetComponent<Storage_Slot>());
    }

    public void Refresh_StorageInven()
    {
        for(int i = 0; i < Inven_ItemSlotList.Count; i++)
        {
            if(Inven_ItemSlotList[i] != null)
            {
                Inven_ItemSlotList[i].Set_SlotInfo(null, 0, false);
            }
        }

        foreach (var item in GlobalValue.User_Inventory)
        {
            Inven_ItemSlotList[item.Value.Get_Item_SlotIndex].Set_SlotInfo(item.Value, item.Value.Get_Item_Amount);
        }

        foreach (var item in GlobalValue.Equipment_Inventory)
        {
            Inven_ItemSlotList[item.Get_Item_SlotIndex].Set_SlotInfo(item, item.Get_Item_Amount);
        }
    }

}

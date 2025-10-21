using System.Collections.Generic;
using UnityEngine;

public interface ISlot
{
    void Set_SlotNum(int num);
}

public class Mgr_Inventory : MonoBehaviour
{
    [Header("Inven_Item_Slot")]
    [SerializeField] List<Inven_Slot> Inven_ItemSlotList = new List<Inven_Slot>();
    [SerializeField] GameObject Inven_ItemSlot_Prefab;
    [SerializeField] Transform ItemSlot_Tr;
    [SerializeField] int ItemSlot_Amount;

    [Header("Inven_Equip_Slot")]
    [SerializeField] List<Equip_Slot> Inven_EquipSlotList = new List<Equip_Slot>();
    [SerializeField] GameObject Inven_EquipSlot_Prefab;
    [SerializeField] Transform EquipSlot_Tr;
    [SerializeField] int EquipSlot_Amount;

    public static Mgr_Inventory Inst = null;

    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }

        // ¾ÆÀÌÅÛ ½½·Ô »ý¼º
        Spawn_Slot<Inven_Slot>(ItemSlot_Amount, Inven_ItemSlot_Prefab, ItemSlot_Tr, Inven_ItemSlotList);

        // ÀåÂø ½½·Ô »ý¼º
        Spawn_Slot<Equip_Slot>(EquipSlot_Amount, Inven_EquipSlot_Prefab, EquipSlot_Tr, Inven_EquipSlotList);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // ½½·Ô »ý¼º
    #region Spawn_Slot
    void Spawn_Slot<T>(int _count, GameObject _prefab, Transform _tr, List<T> _list)
        where T : Component, ISlot
    {
        for (int i = 0; i < _count; i++)
        {
            GameObject slot = Instantiate(_prefab);
            slot.transform.SetParent(_tr);
            T comp = slot.GetComponent<T>();
            comp.Set_SlotNum(i);
            _list.Add(comp);
        }
    }
    #endregion

    #region Refresh Inventory
    public void Refresh_Inventory()
    {
        int index = 0;
        foreach(var item in GlobalValue.User_Inventory)
        {
            Inven_ItemSlotList[index].Set_SlotInfo(item.Value, item.Value.Get_Item_Amount);
            index++;
        }
    }
    #endregion
}

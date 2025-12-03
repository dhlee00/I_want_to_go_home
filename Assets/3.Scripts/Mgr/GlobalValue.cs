using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalValue
{
    // 인벤토리 슬롯
    public static Dictionary<int, Item> User_Inventory = new Dictionary<int, Item>();
    public static List<Item> Equipment_Inventory = new List<Item>();

    // 장착 슬롯
    public static Dictionary<int, Item> User_EquipSlot = new Dictionary<int, Item>();
    public static List<Item> Equipment_EquipSlot = new List<Item>();

    public static string Nickname;


    public static void AddItme(Item ItemData)
    {
        // 인벤토리로 들어가는 아이템 획득 코드
        // 이미 존재한다면
        if (GlobalValue.User_Inventory.ContainsKey(ItemData.Get_Item_Index) == true ||
            GlobalValue.User_EquipSlot.ContainsKey(ItemData.Get_Item_Index) == true)
        {
            // 장비아이템이 아니라면
            if (ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
            {
                // 인벤토리에 존재하면
                if (GlobalValue.User_Inventory.ContainsKey(ItemData.Get_Item_Index) == true)
                {
                    GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount += ItemData.Get_Item_Amount;
                }
                // 장착 슬롯에 존재하면
                else if (GlobalValue.User_EquipSlot.ContainsKey(ItemData.Get_Item_Index) == true)
                {
                    GlobalValue.User_EquipSlot[ItemData.Get_Item_Index].Get_Item_Amount += ItemData.Get_Item_Amount;
                }

            }
            else
            {
                // 장비는 겹쳐지지 않으니까 1
                Add_Inventory(ItemData, 1);
            }
        }
        else // 존재하지 않으면
        {
            Add_Inventory(ItemData, ItemData.Get_Item_Amount);
        }

        // 인벤토리 초기화
        Mgr_Inventory.Inst.Refresh_Inventory();
    }

    static void Add_Inventory(Item ItemData, int _amount)
    {
        int slotNum = -1;
        Item item = new Item(ItemData);

        if (item.Get_ItemType != ITEM_TYPE.EQUIPMENT)
        {
            // 아이템 인벤토리 슬롯 번호 설정
            for (int i = 0; i < Mgr_Inventory.Inst.Get_Inven_ItemSlotList.Count; i++)
            {
                if (Mgr_Inventory.Inst.Get_Inven_ItemSlotList[i].isUse == false)
                {
                    slotNum = i;
                    break;
                }
            }

            // 재료 아이템, 음식 아이템
            GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
            GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount = _amount;
            GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_SlotIndex = slotNum;
        }
        else
        {
            // 아이템 인벤토리 슬롯 번호 설정
            for (int i = 0; i < Mgr_Inventory.Inst.Get_Inven_ItemSlotList.Count; i++)
            {
                if (Mgr_Inventory.Inst.Get_Inven_ItemSlotList[i].isUse == false)
                {
                    slotNum = i;
                    break;
                }
            }

            // 장비 아이템
            item.Get_Item_Amount = _amount;
            item.Get_Item_SlotIndex = slotNum;
            GlobalValue.Equipment_Inventory.Add(item);
        }
    }


    public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}


// 무기의 타입을 결정할 열거형
public enum EWeaponType
{
    Weapon, // 무기
    pickax  // 곡괭이
}
public interface ITakeDamage
{
    public void TakeDamage(GameObject DamageOwner, GameObject DamageObj, float Damage, EWeaponType DamageType);
}
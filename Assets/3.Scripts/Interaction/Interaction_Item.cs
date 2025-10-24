using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class Interaction_Item : Interaction
{
    void Start()
    {
        // �׽�Ʈ
        ItemData = ItemList.Inst.Item_List[0];
    }

    public override void OnInteraction()
    {
        // Debug.Log(this.ItemData.Get_Item_Name);

        // �κ��丮�� ���� ������ ȹ�� �ڵ�
        // �̹� �����Ѵٸ�
        if (GlobalValue.User_Inventory.ContainsKey(ItemData.Get_Item_Index) == true)
        {
            if(ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
            {
                GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount++;
            }
            else
            {
                Add_Inventory();
            }
        }
        else // �������� ������
        {
            Add_Inventory();
        }

        // �κ��丮 �ʱ�ȭ
        Mgr_Inventory.Inst.Refresh_Inventory();
        Destroy(this.gameObject);
    }

    void Add_Inventory()
    {
        Item item = new Item(ItemData);

        GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
        GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount = 1;
    }
}

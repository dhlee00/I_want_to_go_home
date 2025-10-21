using UnityEngine;

public class Interaction_Item : Interaction
{
    void Start()
    {
        // 테스트
        ItemData = ItemList.Inst.Item_List[0];
    }

    public override void OnInteraction()
    {
        // Debug.Log(this.ItemData.Get_Item_Name);

        // 인벤토리로 들어가는 아이템 획득 코드
        // 이미 존재한다면
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
        else // 존재하지 않으면
        {
            Add_Inventory();
        }

        // 인벤토리 초기화
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

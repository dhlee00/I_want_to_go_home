using UnityEngine;

public class Interaction_Item : Interaction
{
    void Start()
    {
        // 테스트(아이템 데이터 설정)
        int Rand = Random.Range(0, ItemList.Inst.Item_List.Count);
        ItemData = ItemList.Inst.Item_List[Rand];
    }

    public override void OnInteraction(int _amount)
    {
        // Debug.Log(this.ItemData.Get_Item_Name);

        // 인벤토리로 들어가는 아이템 획득 코드
        // 이미 존재한다면
        if (GlobalValue.User_Inventory.ContainsKey(ItemData.Get_Item_Index) == true)
        {
            // 장비아이템이 아니라면
            if(ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
            {
                GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount += _amount;
            }
            else
            {
                // 장비는 겹쳐지지 않으니까 1
                Add_Inventory(1);
            }
        }
        else // 존재하지 않으면
        {
            Add_Inventory(_amount);
        }

        // 인벤토리 초기화
        Mgr_Inventory.Inst.Refresh_Inventory();
    }

    void Add_Inventory(int _amount)
    {
        Item item = new Item(ItemData);

        GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
        GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount = _amount;
    }
}

using Unity.Services.Apis.Admin.CloudSave;
using UnityEngine;

public class Interaction_Item : Interaction
{
    // 아이템 정보
    public Item ItemData;


    void Awake()
    {
        InteractionType = EInteractionType.item;
    }

    void Start()
    {
        
    }

    public void SetInteractionItem(int Index, int Amount, Vector3 SpawnPos, Vector3 ForceDir = default)
    {
        ItemData = ItemList.Inst.GetItemData(Index);
        ItemData.Get_Item_Amount = Amount;

        this.gameObject.transform.position = SpawnPos;

        if(ForceDir != default)
        {
            GetComponent<Rigidbody>().AddForce(ForceDir * 4, ForceMode.Impulse);
        }
    }

    public override void OnInteraction()
    {
        // 인벤토리로 들어가는 아이템 획득 코드
        // 이미 존재한다면
        if (GlobalValue.User_Inventory.ContainsKey(ItemData.Get_Item_Index) == true)
        {
            // 장비아이템이 아니라면
            if (ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
            {
                GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount += ItemData.Get_Item_Amount;
            }
            else
            {
                // 장비는 겹쳐지지 않으니까 1
                Add_Inventory(1);
            }
        }
        else // 존재하지 않으면
        {
            Add_Inventory(ItemData.Get_Item_Amount);
        }

        // 인벤토리 초기화
        Mgr_Inventory.Inst.Refresh_Inventory();

        Destroy(this.gameObject);
    }

    void Add_Inventory(int _amount)
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
}

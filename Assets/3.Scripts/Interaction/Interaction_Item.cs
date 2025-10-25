using UnityEngine;

public class Interaction_Item : Interaction
{
    // ������ ����
    public Item ItemData;

    // �׽�Ʈ��
    public int Test = 0;


    void Awake()
    {
        InteractionType = EInteractionType.Itme;
    }

    void Start()
    {
        // �׽�Ʈ(������ ������ ����)
        ItemData = ItemList.Inst.Item_List[Test];
    }

    public override void OnInteraction()
    {
        // �κ��丮�� ���� ������ ȹ�� �ڵ�
        // �̹� �����Ѵٸ�
        if (GlobalValue.User_Inventory.ContainsKey(ItemData.Get_Item_Index) == true)
        {
            // ���������� �ƴ϶��
            if(ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
            {
                GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount += ItemData.Get_Item_Amount;
            }
            else
            {
                // ���� �������� �����ϱ� 1
                Add_Inventory(1);
            }
        }
        else // �������� ������
        {
            Add_Inventory(ItemData.Get_Item_Amount);
        }

        // �κ��丮 �ʱ�ȭ
        Mgr_Inventory.Inst.Refresh_Inventory();

        Destroy(this.gameObject);
    }

    void Add_Inventory(int _amount)
    {
        Item item = new Item(ItemData);

        GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
        GlobalValue.User_Inventory[ItemData.Get_Item_Index].Get_Item_Amount = _amount;
    }
}

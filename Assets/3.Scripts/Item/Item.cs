using UnityEngine;

public enum ITEM_TYPE
{

}

[System.Serializable]
public class Item
{
    // 아이템 이름
    [SerializeField] string Item_Name;
    public string Get_Item_Name { get => Item_Name; }

    // 아이템 타입
    [SerializeField] ITEM_TYPE ItemType;
    public ITEM_TYPE Get_ItemType { get => ItemType; }

    // 아이템 보유 수량
    [SerializeField] int Item_Amount;
    public int Get_Item_Amount { get => Item_Amount; set => Item_Amount = value; }

    // 아이템 보유 슬롯
    [SerializeField] int Item_SlotIndex;
    public int Get_Item_SlotIndex { get => Item_SlotIndex; set => Item_SlotIndex = value; }

    // 아이템 장착 여부
    [SerializeField] bool Item_Equip;
    public bool Get_Item_Equip { get => Item_Equip; set => Item_Equip = value; }

    // 아이템 설명
    [SerializeField] string Item_Desc;
    public string Get_Item_Desc { get => Item_Desc; }

    // 생성자
    #region Constructor
    public Item(string _name, ITEM_TYPE _itemType, string _itemDesc, int _amount = 0, int _slotIndex = -1, bool _isEquip = false)
    {
        Item_Name = _name;
        ItemType = _itemType;
        Item_Desc = _itemDesc;
        Item_Amount = _amount;
        Item_SlotIndex = _slotIndex;
        Item_Equip = _isEquip;
    }
    #endregion
}

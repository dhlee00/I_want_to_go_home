using UnityEngine;

public enum ITEM_TYPE
{
    NONE,
    INGREDIENT,
    FOOD,
    EQUIPMENT,
    TOOL,
}

[System.Serializable]
public class Item
{
    [SerializeField] int Item_Index;
    public int Get_Item_Index { get => Item_Index; }

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

    // 아이템 아이콘
    Sprite Item_Icon;
    public Sprite Get_Item_Icon { get => Item_Icon; }

    [SerializeField] string Item_IconPath;
    public string Get_Item_IconPath { get => Item_IconPath; }

    // 아이템 프리팹
    [SerializeField] string Item_Prefab;
    public string Get_Item_Prefab { get => Item_Prefab; }


    // 생성자
    #region Constructor
    public Item(string _name, ITEM_TYPE _itemType, string _itemDesc, int _amount, string _iconPath, string _item_Prefab = "", int _index = -1, int _slotIndex = -1, bool _isEquip = false)
    {
        Item_Index = _index;
        Item_Name = _name;
        ItemType = _itemType;
        Item_Desc = _itemDesc;
        Item_Amount = _amount;
        Item_SlotIndex = _slotIndex;
        Item_Equip = _isEquip;
        Item_IconPath = _iconPath;
        Item_Prefab = _item_Prefab;

        Load_Image();
    }

    public Item(Item _item)
    {
        Item_Index = _item.Item_Index;
        Item_Name = _item.Item_Name;
        ItemType = _item.ItemType;
        Item_Desc = _item.Item_Desc;
        Item_Amount = _item.Item_Amount;
        Item_SlotIndex = _item.Item_SlotIndex;
        Item_Equip = _item.Item_Equip;
        Item_IconPath = _item.Get_Item_IconPath;

        Item_Icon = Resources.Load<Sprite>(_item.Item_IconPath);
    }
    #endregion

    #region Image_Resources_Load
    public void Load_Image()
    {
        Item_Icon = Resources.Load<Sprite>(Item_IconPath);
    }
    public void Load_Image(string _iconPath)
    {
        Item_Icon = Resources.Load<Sprite>(Item_IconPath);
    }
    #endregion
}

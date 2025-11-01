using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Item_Desc : MonoBehaviour
{
    //[SerializeField] Image Item_Icon;
    [SerializeField] TextMeshProUGUI Item_Name_Text;
    [SerializeField] TextMeshProUGUI Item_Type_Text;
    [SerializeField] TextMeshProUGUI Item_Desc_Text;

    public void Set_UI_Info(Item _item)
    {
        Item_Name_Text.text = _item.Get_Item_Name;
        Item_Type_Text.text = Get_ItemType_Kor(_item);
        Item_Desc_Text.text = _item.Get_Item_Desc;
    }

    string Get_ItemType_Kor(Item _item)
    {
        if (_item.Get_ItemType == ITEM_TYPE.INGREDIENT)
            return "재료 아이템";
        else if (_item.Get_ItemType == ITEM_TYPE.EQUIPMENT)
            return "장비 아이템";
        else if (_item.Get_ItemType == ITEM_TYPE.FOOD)
            return "음식 아이템";
        else if (_item.Get_ItemType == ITEM_TYPE.TOOL)
            return "도구 아이템";

        return "error";
    }
}

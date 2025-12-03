using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class EquipSlot_Info : MonoBehaviour
{
    [SerializeField] Item ItemData;
    [SerializeField] Image Item_Image;
    [SerializeField] TextMeshProUGUI Item_Amount;

    public void Set_UI(Item _item = null)
    {
        if (_item != null)
        {
            ItemData = _item;
            Item_Image.enabled = true;
            Item_Image.sprite = ItemData.Get_Item_Icon;

            // 장비는 수량 표시X
            if (ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
            {
                Item_Amount.text = $"x{ItemData.Get_Item_Amount}";
            }
        }
        else
        {
            ItemData = null;
            Item_Amount.text = "";
            Item_Image.enabled = false;
        }
    }
}

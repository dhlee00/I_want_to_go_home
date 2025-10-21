using UnityEngine;
using UnityEngine.UI;

public class Item_Interact_UI : MonoBehaviour
{
    int ItemAmount = 0;
    public int Get_ItemAmount { get => ItemAmount; }
    [SerializeField] Text ItemName_Text;
    [SerializeField] Image Item_Icon;
    [SerializeField] Interaction interaction;
    public Interaction Get_interaction { get => interaction; }

    void OnDisable()
    {

    }

    public void Set_ItemInfo(string _name, Sprite _sprite, int _amount, Interaction _interaction)
    {
        ItemAmount = _amount;
        Item_Icon.sprite = _sprite;
        interaction = _interaction;

        // 장비가 아니면 수량 표시
        if(interaction.ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
        {
            ItemName_Text.text = $"{_name} x{ItemAmount}";
        }
        else // 장비면 이름만 표시
        {
            ItemName_Text.text = $"{_name}";
        }
        
    }

    // 
    public void Set_Amount(int _count)
    {
        ItemAmount += _count;
        ItemName_Text.text = $"{interaction.ItemData.Get_Item_Name} x{ItemAmount}";
    }
    //
}
